namespace Espada.Protocol.Sync.Contracts
{
    public sealed record SyncPushResponse(long AcceptedThroughSequence, IReadOnlyList<Guid> ConflictIds);
}