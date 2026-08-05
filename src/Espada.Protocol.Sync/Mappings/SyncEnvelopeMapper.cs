using Espada.Domain.Entities;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Espada.Protocol.Sync;
using Espada.Protocol.Sync.Contracts;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Espada.Protocol.Sync.Mappings
{
    public static class SyncEnvelopeMapper
    {
        public static SyncEnvelope ToEnvelope(SyncEvent syncEvent)
        {
            ArgumentNullException.ThrowIfNull(syncEvent);
            return new SyncEnvelope(SyncProtocolConstants.Version, syncEvent.Id.Value, syncEvent.DeviceId.Value,
                syncEvent.Sequence, syncEvent.WorkspaceId.Value, syncEvent.EntityType, syncEvent.EntityId,
                syncEvent.Operation, syncEvent.BaseVersion, syncEvent.OccurredAtUtc, syncEvent.PayloadHash,
                syncEvent.PayloadType, JsonDocument.Parse(syncEvent.PayloadJson).RootElement.Clone());
        }

        public static DomainResult<SyncEvent> ToDomain(SyncEnvelope envelope)
        {
            if (envelope.Version != SyncProtocolConstants.Version || envelope.EventId == Guid.Empty
                || envelope.DeviceId == Guid.Empty || envelope.WorkspaceId == Guid.Empty || envelope.Sequence < 1)
            {
                return DomainResult.Failure<SyncEvent>(new DomainError("Sync.InvalidEnvelope",
                    "The sync envelope identifiers, sequence, or version are invalid."));
            }

            string payloadJson = envelope.Payload.GetRawText();
            string payloadHash = Convert.ToHexStringLower(
                SHA256.HashData(Encoding.UTF8.GetBytes(payloadJson)));
            if (!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(payloadHash),
                    Encoding.ASCII.GetBytes(envelope.PayloadHash.ToLowerInvariant())))
            {
                return DomainResult.Failure<SyncEvent>(new DomainError("Sync.PayloadHashMismatch",
                    "The sync payload hash does not match its content."));
            }

            return SyncEvent.Create(SyncEventId.Create(envelope.EventId), DeviceId.Create(envelope.DeviceId),
                envelope.Sequence, WorkspaceId.Create(envelope.WorkspaceId), envelope.EntityType, envelope.EntityId,
                envelope.Operation, envelope.BaseVersion, envelope.Timestamp, envelope.PayloadType, payloadJson,
                payloadHash);
        }
    }
}