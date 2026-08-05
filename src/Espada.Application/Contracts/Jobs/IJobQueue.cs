using Espada.Application.Enums;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Jobs
{
    public interface IJobQueue
    {
        Task EnqueueAsync(
            ImportJobId importJobId,
            ImportPipelineStageType stage,
            string idempotencyKey,
            CancellationToken cancellationToken = default);

        Task<IngestionJob?> ClaimAsync(
            string leaseOwner,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default);

        Task<IngestionJob?> GetLatestAsync(
            ImportJobId importJobId,
            CancellationToken cancellationToken = default);

        Task CompleteAsync(Guid jobId, string leaseOwner, CancellationToken cancellationToken = default);

        Task RetryAsync(
            Guid jobId,
            string leaseOwner,
            JobFailureCategoryType category,
            string sanitizedError,
            DateTimeOffset availableAtUtc,
            CancellationToken cancellationToken = default);

        Task CancelPendingAsync(ImportJobId importJobId, CancellationToken cancellationToken = default);
    }
}