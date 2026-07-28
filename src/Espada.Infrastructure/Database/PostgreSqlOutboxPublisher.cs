using Espada.Application.Contracts.Jobs;
using Espada.Application.Contracts.Time;
using Espada.Domain.SeedWork;
using Espada.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Database
{
    internal sealed class PostgreSqlOutboxPublisher(
        EspadaDbContext dbContext,
        IDomainEventDispatcherService dispatcherService,
        IClockService clock) : IOutboxPublisher
    {
        private static readonly TimeSpan AvailabilityTolerance = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

        public async Task<bool> PublishNextAsync(string leaseOwner, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(leaseOwner);
            OutboxEnvelope? envelope = await ClaimAsync(leaseOwner, cancellationToken);
            if (envelope is null)
            {
                return false;
            }

            try
            {
                IDomainEvent? domainEvent = DomainEventSerializer.Deserialize(envelope.EventName, envelope.EventVersion,
                    envelope.PayloadJson);
                if (domainEvent is null)
                {
                    await MarkProcessedAsync(envelope.EventId, leaseOwner,
                        $"Unsupported outbox contract '{envelope.EventName}' v{envelope.EventVersion}.",
                        cancellationToken);
                    return true;
                }

                await dispatcherService.PublishAsync(domainEvent, cancellationToken);
                await MarkProcessedAsync(envelope.EventId, leaseOwner, null, cancellationToken);
                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                await ReleaseForRetryAsync(envelope.EventId, leaseOwner,
                    exception.Message.Length <= 4000 ? exception.Message : exception.Message[..4000],
                    cancellationToken);
                return true;
            }
        }

        private async Task<OutboxEnvelope?> ClaimAsync(string leaseOwner, CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                DateTimeOffset now = clock.UtcNow;
                DateTimeOffset availableBefore = now + AvailabilityTolerance;
                Guid? candidateId = await EligibleMessages(now, availableBefore)
                    .OrderBy(message => message.AvailableAtUtc)
                    .ThenBy(message => message.OccurredAtUtc)
                    .ThenBy(message => message.EventId)
                    .Select(message => (Guid?)message.EventId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (candidateId is not Guid eventId)
                {
                    return null;
                }

                DateTimeOffset leaseExpiresAtUtc = now + LeaseDuration;
                int updated = await EligibleMessages(now, availableBefore)
                    .Where(message => message.EventId == eventId)
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(message => message.LeaseOwner, leaseOwner)
                            .SetProperty(message => message.LeaseExpiresAtUtc, leaseExpiresAtUtc)
                            .SetProperty(message => message.Attempt, message => message.Attempt + 1),
                        cancellationToken);
                if (updated == 0)
                {
                    continue;
                }

                return await dbContext.OutboxMessages
                    .AsNoTracking()
                    .Where(message => message.EventId == eventId)
                    .Select(message => new OutboxEnvelope(message.EventId, message.EventName, message.EventVersion,
                        message.PayloadJson))
                    .SingleAsync(cancellationToken);
            }
        }

        private IQueryable<OutboxMessageRecord> EligibleMessages(DateTimeOffset now, DateTimeOffset availableBefore)
        {
            return dbContext.OutboxMessages
                .AsNoTracking()
                .Where(message => message.ProcessedAtUtc == null && message.AvailableAtUtc <= availableBefore &&
                                  (message.LeaseExpiresAtUtc == null || message.LeaseExpiresAtUtc < now));
        }

        private async Task MarkProcessedAsync(Guid eventId, string leaseOwner, string? error,
            CancellationToken cancellationToken)
        {
            DateTimeOffset processedAtUtc = clock.UtcNow;

            await dbContext.OutboxMessages
                .Where(message => message.EventId == eventId && message.LeaseOwner == leaseOwner)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(message => message.ProcessedAtUtc, processedAtUtc)
                        .SetProperty(message => message.LeaseOwner, (string?)null)
                        .SetProperty(message => message.LeaseExpiresAtUtc, (DateTimeOffset?)null)
                        .SetProperty(message => message.SanitizedError, error),
                    cancellationToken);
        }

        private async Task ReleaseForRetryAsync(Guid eventId, string leaseOwner, string error,
            CancellationToken cancellationToken)
        {
            DateTimeOffset availableAtUtc = clock.UtcNow + RetryDelay;

            await dbContext.OutboxMessages
                .Where(message => message.EventId == eventId && message.LeaseOwner == leaseOwner)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(message => message.AvailableAtUtc, availableAtUtc)
                        .SetProperty(message => message.LeaseOwner, (string?)null)
                        .SetProperty(message => message.LeaseExpiresAtUtc, (DateTimeOffset?)null)
                        .SetProperty(message => message.SanitizedError, error),
                    cancellationToken);
        }
    }
}