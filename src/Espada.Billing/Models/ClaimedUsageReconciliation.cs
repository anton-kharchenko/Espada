namespace Espada.Billing.Models
{
    public sealed record ClaimedUsageReconciliation(
        Guid EventId,
        string ProviderCustomerId,
        string Metric,
        long Quantity,
        DateTimeOffset OccurredAtUtc,
        int Attempt);
}