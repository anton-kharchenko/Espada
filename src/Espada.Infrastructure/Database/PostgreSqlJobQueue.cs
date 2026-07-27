using Espada.Application.Contracts.Jobs;
using Espada.Application.Contracts.Time;
using Espada.Application.Enums;
using Espada.Db.Models;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Espada.Infrastructure.Database;

internal sealed class PostgreSqlJobQueue(EspadaDbContext dbContext, IClockService clock) : IJobQueue
{
    private static readonly TimeSpan AvailabilityTolerance = TimeSpan.FromSeconds(1);

    public async Task EnqueueAsync(ImportJobId importJobId, ImportPipelineStageType stage, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(importJobId);
        ArgumentNullException.ThrowIfNull(stage);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);

        IngestionJobs entity = new()
        {
            JobId = Guid.NewGuid(),
            ImportJobId = importJobId.Value,
            Stage = stage.Id,
            IdempotencyKey = idempotencyKey,
            Status = (int)IngestionJobStatusType.Pending,
            AvailableAtUtc = clock.UtcNow,
            CreatedAtUtc = clock.UtcNow
        };
        dbContext.IngestionJobs.Add(entity);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            dbContext.Entry(entity).State = EntityState.Detached;
        }
    }

    public async Task<IngestionJob?> ClaimAsync(string leaseOwner, TimeSpan leaseDuration, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(leaseOwner);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(leaseDuration, TimeSpan.Zero);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DateTimeOffset now = clock.UtcNow;
            DateTimeOffset availableBefore = now + AvailabilityTolerance;
            Guid? candidateId = await EligibleJobs(now, availableBefore)
                .OrderBy(job => job.AvailableAtUtc)
                .ThenBy(job => job.CreatedAtUtc)
                .ThenBy(job => job.JobId)
                .Select(job => (Guid?)job.JobId)
                .FirstOrDefaultAsync(cancellationToken);
            if (candidateId is not Guid jobId)
            {
                return null;
            }

            DateTimeOffset leaseExpiresAtUtc = now + leaseDuration;
            int updated = await EligibleJobs(now, availableBefore)
                .Where(job => job.JobId == jobId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(job => job.Status, (int)IngestionJobStatusType.Running)
                        .SetProperty(job => job.Attempt, job => job.Attempt + 1)
                        .SetProperty(job => job.LeaseOwner, leaseOwner)
                        .SetProperty(job => job.LeaseExpiresAtUtc, leaseExpiresAtUtc)
                        .SetProperty(job => job.StartedAtUtc, job => job.StartedAtUtc ?? now),
                    cancellationToken);
            if (updated == 0)
            {
                continue;
            }

            IngestionJobs claimed = await dbContext.IngestionJobs
                .AsNoTracking()
                .SingleAsync(job => job.JobId == jobId, cancellationToken);
            return Map(claimed);
        }
    }

    public async Task<IngestionJob?> GetLatestAsync(ImportJobId importJobId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(importJobId);

        IngestionJobs? entity = await dbContext.IngestionJobs
            .AsNoTracking()
            .Where(job => job.ImportJobId == importJobId.Value)
            .OrderByDescending(job => job.CreatedAtUtc)
            .ThenByDescending(job => job.JobId)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public Task CompleteAsync(Guid jobId, string leaseOwner, CancellationToken cancellationToken = default) =>
        UpdateTerminalAsync(jobId, leaseOwner, IngestionJobStatusType.Succeeded, null, null, cancellationToken);

    public async Task RetryAsync(Guid jobId, string leaseOwner, JobFailureCategoryType category, string sanitizedError, DateTimeOffset availableAtUtc, CancellationToken cancellationToken = default)
    {
        IngestionJobStatusType status = category switch
        {
            JobFailureCategoryType.Transient => IngestionJobStatusType.Pending,
            JobFailureCategoryType.Cancelled => IngestionJobStatusType.Cancelled,
            JobFailureCategoryType.Poison => IngestionJobStatusType.Poisoned,
            _ => IngestionJobStatusType.Failed
        };
        DateTimeOffset? completedAtUtc = status == IngestionJobStatusType.Pending
            ? null
            : clock.UtcNow;

        await dbContext.IngestionJobs
            .Where(job => job.JobId == jobId && job.LeaseOwner == leaseOwner)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(job => job.Status, (int)status)
                    .SetProperty(job => job.AvailableAtUtc, availableAtUtc)
                    .SetProperty(job => job.FailureCategory, (int)category)
                    .SetProperty(job => job.SanitizedError, sanitizedError)
                    .SetProperty(job => job.LeaseOwner, (string?)null)
                    .SetProperty(job => job.LeaseExpiresAtUtc, (DateTimeOffset?)null)
                    .SetProperty(job => job.CompletedAtUtc, completedAtUtc),
                cancellationToken);
    }

    public async Task CancelPendingAsync(ImportJobId importJobId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(importJobId);
        DateTimeOffset completedAtUtc = clock.UtcNow;

        await dbContext.IngestionJobs
            .Where(job => job.ImportJobId == importJobId.Value && job.Status == (int)IngestionJobStatusType.Pending)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(job => job.Status, (int)IngestionJobStatusType.Cancelled)
                    .SetProperty(job => job.CompletedAtUtc, completedAtUtc)
                    .SetProperty(job => job.LeaseOwner, (string?)null)
                    .SetProperty(job => job.LeaseExpiresAtUtc, (DateTimeOffset?)null),
                cancellationToken);
    }

    private IQueryable<IngestionJobs> EligibleJobs(DateTimeOffset now, DateTimeOffset availableBefore) =>
        dbContext.IngestionJobs
            .AsNoTracking()
            .Where(job => job.AvailableAtUtc <= availableBefore && (job.Status == (int)IngestionJobStatusType.Pending || (job.Status == (int)IngestionJobStatusType.Running && job.LeaseExpiresAtUtc < now)));

    private async Task UpdateTerminalAsync(Guid jobId, string leaseOwner, IngestionJobStatusType status, JobFailureCategoryType? failureCategory, string? sanitizedError, CancellationToken cancellationToken)
    {
        DateTimeOffset completedAtUtc = clock.UtcNow;
        int? failureCategoryId = failureCategory is null ? null : (int)failureCategory.Value;

        await dbContext.IngestionJobs
            .Where(job => job.JobId == jobId && job.LeaseOwner == leaseOwner)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(job => job.Status, (int)status)
                    .SetProperty(job => job.CompletedAtUtc, completedAtUtc)
                    .SetProperty(job => job.LeaseOwner, (string?)null)
                    .SetProperty(job => job.LeaseExpiresAtUtc, (DateTimeOffset?)null)
                    .SetProperty(job => job.FailureCategory, failureCategoryId)
                    .SetProperty(job => job.SanitizedError, sanitizedError),
                cancellationToken);
    }

    private static IngestionJob Map(IngestionJobs entity) =>
        new(
            entity.JobId,
            ImportJobId.Create(entity.ImportJobId),
            ReadStage(entity.Stage),
            entity.IdempotencyKey,
            entity.Attempt,
            (IngestionJobStatusType)entity.Status,
            entity.AvailableAtUtc,
            entity.LeaseOwner,
            entity.LeaseExpiresAtUtc,
            entity.CreatedAtUtc,
            entity.StartedAtUtc,
            entity.CompletedAtUtc,
            entity.FailureCategory is null ? null : (JobFailureCategoryType)entity.FailureCategory.Value,
            entity.SanitizedError);

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        };

    private static ImportPipelineStageType ReadStage(int id) => Enumeration.FromId<ImportPipelineStageType>(id);
}