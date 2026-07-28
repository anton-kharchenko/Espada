namespace Espada.Api.Contracts.Responses.Billing
{
    public sealed record BillingStatusResponse(
        string Plan,
        string SubscriptionStatus,
        string AccessState,
        DateTimeOffset? PaymentFailedAtUtc,
        bool ExportAvailable);
}