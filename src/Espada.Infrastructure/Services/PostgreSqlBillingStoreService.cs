using Espada.Application.Contracts.Time;
using Espada.Billing.Contracts;
using Espada.Billing.Enums;
using Espada.Billing.Models;
using Espada.Db.Models;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Espada.Infrastructure.Services;

internal sealed class PostgreSqlBillingStoreService(EspadaDbContext dbContext, IClockService clock) : IBillingStoreService
{
    private static readonly TimeSpan AvailabilityTolerance = TimeSpan.FromSeconds(1);

    public Task<BillingCustomerSnapshot?> GetCustomerByWorkspaceAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default) =>
        dbContext.BillingCustomers
            .AsNoTracking()
            .Where(customer => customer.WorkspaceId == workspaceId)
            .Select(customer => new BillingCustomerSnapshot(
                customer.WorkspaceId,
                customer.ProviderCustomerId,
                customer.ProviderSubscriptionId,
                (CloudBillingPlanType)customer.Plan,
                customer.SubscriptionStatus,
                customer.PaymentFailedAtUtc,
                customer.LastProviderEventAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<bool> AddPaymentEventAsync(PaymentEventEnvelope paymentEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(paymentEvent);

        PaymentEvents entity = new()
        {
            ProviderEventId = paymentEvent.ProviderEventId,
            EventType = paymentEvent.EventType,
            ApiVersion = paymentEvent.ApiVersion,
            PayloadJson = paymentEvent.PayloadJson,
            ProviderCreatedAtUtc = paymentEvent.ProviderCreatedAtUtc,
            ReceivedAtUtc = paymentEvent.ReceivedAtUtc,
            AvailableAtUtc = paymentEvent.ReceivedAtUtc,
            Status = (int)PaymentEventStatusType.Pending
        };
        dbContext.PaymentEvents.Add(entity);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            dbContext.Entry(entity).State = EntityState.Detached;
            return true;
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            dbContext.Entry(entity).State = EntityState.Detached;
            return false;
        }
    }

    public async Task<ClaimedPaymentEvent?> ClaimPaymentEventAsync(string workerId, TimeSpan leaseDuration, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);
        ValidateLeaseDuration(leaseDuration);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DateTimeOffset now = clock.UtcNow;
            DateTimeOffset availableBefore = now + AvailabilityTolerance;
            string? candidateId = await EligiblePaymentEvents(now, availableBefore)
                .OrderBy(paymentEvent => paymentEvent.ReceivedAtUtc)
                .ThenBy(paymentEvent => paymentEvent.ProviderEventId)
                .Select(paymentEvent => paymentEvent.ProviderEventId)
                .FirstOrDefaultAsync(cancellationToken);
            if (candidateId is null)
            {
                return null;
            }

            DateTimeOffset leaseExpiresAtUtc = now + leaseDuration;
            int updated = await EligiblePaymentEvents(now, availableBefore)
                .Where(paymentEvent => paymentEvent.ProviderEventId == candidateId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(paymentEvent => paymentEvent.Status, (int)PaymentEventStatusType.Processing)
                        .SetProperty(paymentEvent => paymentEvent.Attempt, paymentEvent => paymentEvent.Attempt + 1)
                        .SetProperty(paymentEvent => paymentEvent.LeaseOwner, workerId)
                        .SetProperty(paymentEvent => paymentEvent.LeaseExpiresAtUtc, leaseExpiresAtUtc)
                        .SetProperty(paymentEvent => paymentEvent.SanitizedError, (string?)null),
                    cancellationToken);
            if (updated == 0)
            {
                continue;
            }

            return await dbContext.PaymentEvents
                .AsNoTracking()
                .Where(paymentEvent => paymentEvent.ProviderEventId == candidateId)
                .Select(paymentEvent => new ClaimedPaymentEvent(paymentEvent.ProviderEventId, paymentEvent.EventType, paymentEvent.PayloadJson, paymentEvent.Attempt))
                .SingleAsync(cancellationToken);
        }
    }

    public async Task ApplyCustomerUpdateAsync(BillingCustomerUpdate update, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);

        if (update is { WorkspaceId: { } workspaceId, Plan: { } plan })
        {
            await UpsertCustomerAsync(workspaceId, plan, update, cancellationToken);
            return;
        }

        await UpdateCustomerByProviderIdAsync(update, cancellationToken);
    }

    public async Task MarkPaymentEventProcessedAsync(string providerEventId, string workerId, CancellationToken cancellationToken = default)
    {
        DateTimeOffset processedAtUtc = clock.UtcNow;

        await ProcessingPaymentEvent(providerEventId, workerId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(paymentEvent => paymentEvent.Status, (int)PaymentEventStatusType.Processed)
                    .SetProperty(paymentEvent => paymentEvent.ProcessedAtUtc, processedAtUtc)
                    .SetProperty(paymentEvent => paymentEvent.LeaseOwner, (string?)null)
                    .SetProperty(paymentEvent => paymentEvent.LeaseExpiresAtUtc, (DateTimeOffset?)null)
                    .SetProperty(paymentEvent => paymentEvent.SanitizedError, (string?)null),
                cancellationToken);
    }

    public async Task MarkPaymentEventFailedAsync(string providerEventId, string workerId, bool retryable, DateTimeOffset availableAtUtc, string sanitizedError, CancellationToken cancellationToken = default)
    {
        PaymentEventStatusType status = retryable ? PaymentEventStatusType.Pending : PaymentEventStatusType.Failed;

        await ProcessingPaymentEvent(providerEventId, workerId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(paymentEvent => paymentEvent.Status, (int)status)
                    .SetProperty(paymentEvent => paymentEvent.AvailableAtUtc, availableAtUtc)
                    .SetProperty(paymentEvent => paymentEvent.LeaseOwner, (string?)null)
                    .SetProperty(paymentEvent => paymentEvent.LeaseExpiresAtUtc, (DateTimeOffset?)null)
                    .SetProperty(paymentEvent => paymentEvent.SanitizedError, sanitizedError),
                cancellationToken);
    }

    public async Task<ClaimedUsageReconciliation?> ClaimUsageReconciliationAsync(string workerId, TimeSpan leaseDuration, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);
        ValidateLeaseDuration(leaseDuration);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DateTimeOffset now = clock.UtcNow;
            DateTimeOffset availableBefore = now + AvailabilityTolerance;
            Guid? candidateId = await EligibleUsageReconciliations(
                    now,
                    availableBefore)
                .OrderBy(message => message.AvailableAtUtc)
                .ThenBy(message => message.EventId)
                .Select(message => (Guid?)message.EventId)
                .FirstOrDefaultAsync(cancellationToken);
            if (candidateId is not Guid eventId)
            {
                return null;
            }

            DateTimeOffset leaseExpiresAtUtc = now + leaseDuration;
            int updated = await EligibleUsageReconciliations(now, availableBefore)
                .Where(message => message.EventId == eventId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(message => message.Status, (int)UsageReconciliationStatusType.Processing)
                        .SetProperty(message => message.Attempt, message => message.Attempt + 1)
                        .SetProperty(message => message.LeaseOwner, workerId)
                        .SetProperty(message => message.LeaseExpiresAtUtc, leaseExpiresAtUtc)
                        .SetProperty(message => message.SanitizedError, (string?)null),
                    cancellationToken);
            if (updated == 0)
            {
                continue;
            }

            ClaimedUsageReconciliation? claimed = await (
                    from message in dbContext.UsageReconciliationOutbox.AsNoTracking()
                    join ledger in dbContext.UsageLedgerEntries.AsNoTracking()
                        on message.LedgerEntryId equals ledger.EntryId
                    join customer in dbContext.BillingCustomers.AsNoTracking()
                        on ledger.WorkspaceId equals customer.WorkspaceId
                    where message.EventId == eventId
                    select new ClaimedUsageReconciliation(
                        message.EventId,
                        customer.ProviderCustomerId,
                        ledger.Metric,
                        ledger.Quantity,
                        ledger.OccurredAtUtc,
                        message.Attempt))
                .SingleOrDefaultAsync(cancellationToken);
            if (claimed is not null)
            {
                return claimed;
            }

            await MarkUsageReconciliationFailedAsync(eventId, workerId, retryable: false, now, "Usage reconciliation references missing billing data.", cancellationToken);
        }
    }

    public async Task MarkUsageReconciledAsync(Guid eventId, string workerId, CancellationToken cancellationToken = default)
    {
        DateTimeOffset processedAtUtc = clock.UtcNow;

        await ProcessingUsageReconciliation(eventId, workerId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(message => message.Status, (int)UsageReconciliationStatusType.Processed)
                    .SetProperty(message => message.ProcessedAtUtc, processedAtUtc)
                    .SetProperty(message => message.LeaseOwner, (string?)null)
                    .SetProperty(message => message.LeaseExpiresAtUtc, (DateTimeOffset?)null)
                    .SetProperty(message => message.SanitizedError, (string?)null),
                cancellationToken);
    }

    public async Task MarkUsageReconciliationFailedAsync(Guid eventId, string workerId, bool retryable, DateTimeOffset availableAtUtc, string sanitizedError, CancellationToken cancellationToken = default)
    {
        UsageReconciliationStatusType status = retryable ? UsageReconciliationStatusType.Pending : UsageReconciliationStatusType.Failed;

        await ProcessingUsageReconciliation(eventId, workerId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(message => message.Status, (int)status)
                    .SetProperty(message => message.AvailableAtUtc, availableAtUtc)
                    .SetProperty(message => message.LeaseOwner, (string?)null)
                    .SetProperty(message => message.LeaseExpiresAtUtc, (DateTimeOffset?)null)
                    .SetProperty(message => message.SanitizedError, sanitizedError),
                cancellationToken);
    }

    private IQueryable<PaymentEvents> EligiblePaymentEvents(DateTimeOffset now, DateTimeOffset availableBefore) =>
        dbContext.PaymentEvents
            .AsNoTracking()
            .Where(paymentEvent => paymentEvent.AvailableAtUtc <= availableBefore && (paymentEvent.Status == (int)PaymentEventStatusType.Pending || (paymentEvent.Status == (int)PaymentEventStatusType.Processing && paymentEvent.LeaseExpiresAtUtc <= now)));

    private IQueryable<UsageReconciliationOutbox> EligibleUsageReconciliations(
        DateTimeOffset now,
        DateTimeOffset availableBefore) =>
        dbContext.UsageReconciliationOutbox
            .AsNoTracking()
            .Where(message => message.AvailableAtUtc <= availableBefore && (message.Status == (int)UsageReconciliationStatusType.Pending || (message.Status == (int)UsageReconciliationStatusType.Processing && message.LeaseExpiresAtUtc <= now)))
            .Where(message => dbContext.UsageLedgerEntries.Any(ledger => ledger.EntryId == message.LedgerEntryId && dbContext.BillingCustomers.Any(customer => customer.WorkspaceId == ledger.WorkspaceId)));

    private IQueryable<PaymentEvents> ProcessingPaymentEvent(string providerEventId, string workerId) =>
        dbContext.PaymentEvents.Where(paymentEvent =>
            paymentEvent.ProviderEventId == providerEventId && paymentEvent.Status == (int)PaymentEventStatusType.Processing && paymentEvent.LeaseOwner == workerId);

    private IQueryable<UsageReconciliationOutbox> ProcessingUsageReconciliation(Guid eventId, string workerId) =>
        dbContext.UsageReconciliationOutbox.Where(message =>
            message.EventId == eventId && message.Status == (int)UsageReconciliationStatusType.Processing && message.LeaseOwner == workerId);

    private async Task UpsertCustomerAsync(Guid workspaceId, CloudBillingPlanType plan, BillingCustomerUpdate update, CancellationToken cancellationToken)
    {
        int updated = await UpdateCustomerByWorkspaceAsync(workspaceId, plan, update, cancellationToken);
        if (updated > 0 || await dbContext.BillingCustomers.AnyAsync(customer => customer.WorkspaceId == workspaceId, cancellationToken))
        {
            return;
        }

        BillingCustomers entity = new()
        {
            WorkspaceId = workspaceId,
            ProviderCustomerId = update.ProviderCustomerId,
            ProviderSubscriptionId = update.ProviderSubscriptionId,
            Plan = (int)plan,
            SubscriptionStatus = update.SubscriptionStatus,
            PaymentFailedAtUtc = update.PaymentFailedAtUtc,
            LastProviderEventAtUtc = update.ProviderEventAtUtc
        };
        dbContext.BillingCustomers.Add(entity);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            dbContext.Entry(entity).State = EntityState.Detached;
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            dbContext.Entry(entity).State = EntityState.Detached;
            await UpdateCustomerByWorkspaceAsync(workspaceId, plan, update, cancellationToken);
        }
    }

    private Task<int> UpdateCustomerByWorkspaceAsync(Guid workspaceId, CloudBillingPlanType plan, BillingCustomerUpdate update, CancellationToken cancellationToken) =>
        dbContext.BillingCustomers
            .Where(customer => customer.WorkspaceId == workspaceId && customer.LastProviderEventAtUtc <= update.ProviderEventAtUtc)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(customer => customer.ProviderCustomerId, update.ProviderCustomerId)
                    .SetProperty(customer => customer.ProviderSubscriptionId, customer => update.ProviderSubscriptionId ?? customer.ProviderSubscriptionId)
                    .SetProperty(customer => customer.Plan, (int)plan)
                    .SetProperty(customer => customer.SubscriptionStatus, update.SubscriptionStatus)
                    .SetProperty(customer => customer.PaymentFailedAtUtc, update.PaymentFailedAtUtc)
                    .SetProperty(customer => customer.LastProviderEventAtUtc, update.ProviderEventAtUtc),
                cancellationToken);

    private Task<int> UpdateCustomerByProviderIdAsync(BillingCustomerUpdate update, CancellationToken cancellationToken) =>
        dbContext.BillingCustomers
            .Where(customer => customer.ProviderCustomerId == update.ProviderCustomerId && customer.LastProviderEventAtUtc <= update.ProviderEventAtUtc)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(customer => customer.ProviderSubscriptionId, customer => update.ProviderSubscriptionId ?? customer.ProviderSubscriptionId)
                    .SetProperty(customer => customer.Plan, customer => update.Plan.HasValue ? (int)update.Plan.Value : customer.Plan)
                    .SetProperty(customer => customer.SubscriptionStatus, update.SubscriptionStatus)
                    .SetProperty(customer => customer.PaymentFailedAtUtc, update.PaymentFailedAtUtc)
                    .SetProperty(customer => customer.LastProviderEventAtUtc, update.ProviderEventAtUtc),
                cancellationToken);

    private static void ValidateLeaseDuration(TimeSpan leaseDuration) => ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(leaseDuration, TimeSpan.Zero);

    private static bool IsUniqueViolation(DbUpdateException exception) => exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}