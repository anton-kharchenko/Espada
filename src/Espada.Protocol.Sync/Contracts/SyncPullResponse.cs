namespace Espada.Protocol.Sync.Contracts
{
    public sealed record SyncPullResponse(string Cursor, IReadOnlyList<SyncEnvelope> Events);
}