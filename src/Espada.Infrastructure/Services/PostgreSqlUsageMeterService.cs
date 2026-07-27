using Espada.Application.Contracts.Billing;
using Espada.Billing;
using Espada.Db.Models;
using Espada.Infrastructure.Database;
using Microsoft.Extensions.Options;

namespace Espada.Infrastructure.Services;

internal sealed class PostgreSqlUsageMeterService(EspadaDbContext dbContext, IOptions<BillingOptions> options) : IUsageMeterService
{
    public async Task RecordAsync(Guid workspaceId, string metric, long quantity, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        if (!options.Value.Enabled || quantity <= 0)
        {
            return;
        }

        Guid entryId = Guid.NewGuid();
        
        await dbContext.UsageLedgerEntries.AddAsync(new UsageLedgerEntries { EntryId = entryId, WorkspaceId = workspaceId, Metric = metric, Quantity = quantity, IdempotencyKey = idempotencyKey, OccurredAtUtc = DateTimeOffset.UtcNow }, cancellationToken);
        
        await dbContext.UsageReconciliationOutbox.AddAsync(new UsageReconciliationOutbox { EventId = Guid.NewGuid(), LedgerEntryId = entryId, AvailableAtUtc = DateTimeOffset.UtcNow, Status = 1 }, cancellationToken);
    }
}