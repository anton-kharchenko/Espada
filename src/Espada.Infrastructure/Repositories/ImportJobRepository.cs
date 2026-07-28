using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class ImportJobRepository(EspadaDbContext dbContext) : IImportJobRepository
    {
        public async Task AddAsync(ImportJob importJob, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(importJob);

            await dbContext.ImportJobs.AddAsync(importJob, cancellationToken);
        }

        public async Task<ImportJob?> GetByIdAsync(ImportJobId importJobId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(importJobId);

            return await dbContext.ImportJobs.FindAsync([importJobId], cancellationToken);
        }

        public Task<ImportJob?> GetByIdempotencyKeyAsync(WorkspaceId workspaceId, string idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);
            ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);

            return dbContext.ImportJobs.SingleOrDefaultAsync(
                importJob => importJob.WorkspaceId == workspaceId && importJob.IdempotencyKey == idempotencyKey,
                cancellationToken);
        }

        public async Task<IReadOnlyList<ImportJob>> ListByWorkspaceIdAsync(
            WorkspaceId workspaceId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);

            return await dbContext.ImportJobs
                .AsNoTracking()
                .Where(importJob => importJob.WorkspaceId == workspaceId)
                .OrderByDescending(importJob => importJob.RequestedAtUtc)
                .ThenBy(importJob => importJob.Id)
                .ToArrayAsync(cancellationToken);
        }

        public Task<bool> IsBlobReferencedByOtherImportAsync(ImportJobId importJobId, string blobHash,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(importJobId);
            ArgumentException.ThrowIfNullOrWhiteSpace(blobHash);
            return dbContext.ImportJobs
                .AsNoTracking()
                .AnyAsync(
                    importJob => importJob.Id != importJobId
                                 && (importJob.RawBlobHash == blobHash
                                     || importJob.ParsedBlobHash == blobHash),
                    cancellationToken);
        }
    }
}