namespace Espada.Infrastructure.Models
{
    internal sealed class OutboxMessageRecord
    {
        private OutboxMessageRecord()
        {
        }

        public OutboxMessageRecord(
            Guid eventId,
            string eventName,
            int eventVersion,
            string payloadJson,
            DateTimeOffset occurredAtUtc)
        {
            EventId = eventId;
            EventName = eventName;
            EventVersion = eventVersion;
            PayloadJson = payloadJson;
            OccurredAtUtc = occurredAtUtc;
            AvailableAtUtc = occurredAtUtc;
        }

        public Guid EventId { get; private set; }

        public string EventName { get; private set; } = string.Empty;

        public int EventVersion { get; private set; }

        public string PayloadJson { get; private set; } = string.Empty;

        public DateTimeOffset OccurredAtUtc { get; private set; }

        public DateTimeOffset AvailableAtUtc { get; private set; }

        public int Attempt { get; private set; }

        public string? LeaseOwner { get; private set; }

        public DateTimeOffset? LeaseExpiresAtUtc { get; private set; }

        public DateTimeOffset? ProcessedAtUtc { get; private set; }

        public string? SanitizedError { get; private set; }
    }
}