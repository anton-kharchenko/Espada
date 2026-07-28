using Espada.Billing.Models;

namespace Espada.Billing.Contracts
{
    public interface IBillingStoreService
    {
        Task<BillingCustomerSnapshot?> GetCustomerByWorkspaceAsync(
            Guid workspaceId,
            CancellationToken cancellationToken = default);

        Task<bool> AddPaymentEventAsync(
            PaymentEventEnvelope paymentEvent,
            CancellationToken cancellationToken = default);

        Task<ClaimedPaymentEvent?> ClaimPaymentEventAsync(
            string workerId,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default);

        Task ApplyCustomerUpdateAsync(
            BillingCustomerUpdate update,
            CancellationToken cancellationToken = default);

        Task MarkPaymentEventProcessedAsync(
            string providerEventId,
            string workerId,
            CancellationToken cancellationToken = default);

        Task MarkPaymentEventFailedAsync(
            string providerEventId,
            string workerId,
            bool retryable,
            DateTimeOffset availableAtUtc,
            string sanitizedError,
            CancellationToken cancellationToken = default);

        Task<ClaimedUsageReconciliation?> ClaimUsageReconciliationAsync(
            string workerId,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default);

        Task MarkUsageReconciledAsync(
            Guid eventId,
            string workerId,
            CancellationToken cancellationToken = default);

        Task MarkUsageReconciliationFailedAsync(
            Guid eventId,
            string workerId,
            bool retryable,
            DateTimeOffset availableAtUtc,
            string sanitizedError,
            CancellationToken cancellationToken = default);
    }
}