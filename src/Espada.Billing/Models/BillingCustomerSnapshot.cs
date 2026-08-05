using Espada.Billing.Enums;

namespace Espada.Billing.Models
{
    public sealed record BillingCustomerSnapshot(
        Guid WorkspaceId,
        string ProviderCustomerId,
        string? ProviderSubscriptionId,
        CloudBillingPlanType Plan,
        string SubscriptionStatus,
        DateTimeOffset? PaymentFailedAtUtc,
        DateTimeOffset LastProviderEventAtUtc)
    {
        public BillingAccessStateType GetAccessState(DateTimeOffset now)
        {
            return PaymentFailedAtUtc switch
            {
                null => BillingAccessStateType.Active,
                { } failedAt when now >= failedAt.AddDays(30) => BillingAccessStateType.SyncDisabled,
                { } failedAt when now >= failedAt.AddDays(7) => BillingAccessStateType.ReadOnly,
                _ => BillingAccessStateType.Grace
            };
        }
    }
}