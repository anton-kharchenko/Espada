namespace Espada.Protocol.Sync.Contracts
{
    public sealed record SyncPushRequest(Guid DeviceId, IReadOnlyList<SyncEnvelope> Events);
}