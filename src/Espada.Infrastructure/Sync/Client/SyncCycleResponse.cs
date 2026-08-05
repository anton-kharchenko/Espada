namespace Espada.Infrastructure.Sync.Client
{
    public sealed record SyncCycleResponse(
        int PushedEvents,
        int PulledEvents,
        IReadOnlyList<Guid> ConflictIds,
        string Cursor);
}