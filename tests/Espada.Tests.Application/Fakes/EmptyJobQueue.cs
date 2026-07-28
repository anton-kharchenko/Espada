using Espada.Application.Contracts.Jobs;
using Espada.Application.Enums;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class EmptyJobQueue : IJobQueue
    {
        public Task<IngestionJob?> GetLatestAsync(
            ImportJobId importJobId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IngestionJob?>(null);
        }

        public Task EnqueueAsync(
            ImportJobId importJobId,
            ImportPipelineStageType stage,
            string idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IngestionJob?> ClaimAsync(
            string leaseOwner,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task CompleteAsync(
            Guid jobId,
            string leaseOwner,
            CancellationToken cancellationToken = default)
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

        public Task CancelPendingAsync(
            ImportJobId importJobId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}