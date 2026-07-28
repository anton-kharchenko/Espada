using Espada.Application.Contracts.Billing;
using Espada.Application.Contracts.Time;
using Espada.Billing.Enums;
using Espada.Billing.Options;
using Espada.Db.Models;
using Espada.Infrastructure.Database;
using Microsoft.Extensions.Options;

namespace Espada.Infrastructure.Services
{
    internal sealed class PostgreSqlUsageMeterService(
        EspadaDbContext dbContext,
        IOptions<BillingOptions> options,
        IClockService clock) : IUsageMeterService
    {
        public async Task RecordAsync(Guid workspaceId, string metric, long quantity, string idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            if (!options.Value.Enabled || quantity <= 0)
            {
                return;
            }

            Guid entryId = Guid.NewGuid();
            DateTimeOffset occurredAtUtc = clock.UtcNow;

            await dbContext.UsageLedgerEntries.AddAsync(
                new UsageLedgerEntries
                {
                    EntryId = entryId,
                    WorkspaceId = workspaceId,
                    Metric = metric,
                    Quantity = quantity,
                    IdempotencyKey = idempotencyKey,
                    OccurredAtUtc = occurredAtUtc
                },
                cancellationToken);

            await dbContext.UsageReconciliationOutbox.AddAsync(
                new UsageReconciliationOutbox
                {
                    EventId = Guid.NewGuid(),
                    LedgerEntryId = entryId,
                    AvailableAtUtc = occurredAtUtc,
                    Status = (int)UsageReconciliationStatusType.Pending
                },
                cancellationToken);
        }
    }
}