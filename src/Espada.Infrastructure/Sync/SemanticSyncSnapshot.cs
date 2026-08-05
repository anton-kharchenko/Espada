namespace Espada.Infrastructure.Sync
{
    internal sealed record SemanticSyncSnapshot(Guid WorkspaceId, string EntityType, Guid EntityId, string Operation,
        uint? BaseVersion, DateTimeOffset Timestamp, string PayloadType, string PayloadJson, string PayloadHash);
}