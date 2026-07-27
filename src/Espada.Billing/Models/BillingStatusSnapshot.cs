using Espada.Billing.Enums;

namespace Espada.Billing.Models;

public sealed record BillingStatusSnapshot(
    CloudBillingPlanType Plan,
    string SubscriptionStatus,
    BillingAccessStateType AccessState,
    DateTimeOffset? PaymentFailedAtUtc,
    bool ExportAvailable);