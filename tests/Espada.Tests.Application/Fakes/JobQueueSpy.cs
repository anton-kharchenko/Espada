using Espada.Application.Contracts.Jobs;
using Espada.Application.Enums;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class JobQueueSpy : IJobQueue
    {
        public ImportJobId? CancelledImportJobId { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task CancelPendingAsync(ImportJobId importJobId, CancellationToken cancellationToken = default)
        {
            CancelledImportJobId = importJobId;
            CancellationToken = cancellationToken;
            return Task.CompletedTask;
        }

        public Task<IngestionJob?> GetLatestAsync(ImportJobId importJobId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IngestionJob?>(null);
        }

        public Task EnqueueAsync(ImportJobId importJobId, ImportPipelineStageType stage, string idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IngestionJob?> ClaimAsync(string leaseOwner, TimeSpan leaseDuration,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task CompleteAsync(Guid jobId, string leaseOwner, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task RetryAsync(
            Guid jobId,
            string leaseOwner,
            JobFailureCategoryType category,
            string sanitizedError,
            DateTimeOffset availableAtUtc,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}