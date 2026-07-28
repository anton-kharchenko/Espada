namespace Espada.Billing.Models
{
    public sealed record PaymentEventEnvelope(
        string ProviderEventId,
        string EventType,
        string ApiVersion,
        string PayloadJson,
        DateTimeOffset ProviderCreatedAtUtc,
        DateTimeOffset ReceivedAtUtc);
}