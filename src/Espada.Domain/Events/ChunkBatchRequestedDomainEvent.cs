using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events
{
    public sealed record ChunkBatchRequestedDomainEvent(
        ChunkBatchId ChunkBatchId,
        WorkspaceId WorkspaceId,
        ArtifactId ArtifactId,
        ArtifactRevisionId ArtifactRevisionId,
        ChunkingStrategyType Strategy,
        string StrategyVersion,
        DateTimeOffset RequestedAtUtc) : IDomainEvent;
}