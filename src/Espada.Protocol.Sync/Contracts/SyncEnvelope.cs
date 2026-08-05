using System.Text.Json;

namespace Espada.Protocol.Sync.Contracts
{
    public sealed record SyncEnvelope(int Version, Guid EventId, Guid DeviceId, long Sequence, Guid WorkspaceId,
        string EntityType, Guid EntityId, string Operation, uint? BaseVersion, DateTimeOffset Timestamp,
        string PayloadHash, string PayloadType, JsonElement Payload);
}