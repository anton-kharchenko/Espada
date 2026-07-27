using Espada.Billing.Enums;

namespace Espada.Billing.Models;

public sealed record BillingCustomerUpdate(
    Guid? WorkspaceId,
    string ProviderCustomerId,
    string? ProviderSubscriptionId,
    CloudBillingPlanType? Plan,
    string SubscriptionStatus,
    DateTimeOffset? PaymentFailedAtUtc,
    DateTimeOffset ProviderEventAtUtc);