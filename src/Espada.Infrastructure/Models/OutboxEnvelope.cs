namespace Espada.Infrastructure.Models
{
    internal sealed record OutboxEnvelope(
        Guid EventId,
        string EventName,
        int EventVersion,
        string PayloadJson);
}