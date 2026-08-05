using Espada.Billing.Enums;
using Espada.Billing.Models;

namespace Espada.Billing.Contracts
{
    public interface IStripeBillingProvider
    {
        Task<HostedBillingSession> CreateCheckoutAsync(
            Guid workspaceId,
            string? customerId,
            CloudBillingPlanType plan,
            string idempotencyKey,
            CancellationToken cancellationToken = default);

        Task<HostedBillingSession> CreateCustomerPortalAsync(
            string customerId,
            string idempotencyKey,
            CancellationToken cancellationToken = default);
    }
}