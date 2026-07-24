using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events;

public sealed record ChunkCreatedDomainEvent(
    ChunkId ChunkId,
    ChunkBatchId BatchId,
    WorkspaceId WorkspaceId,
    ArtifactId ArtifactId,
    ArtifactRevisionId ArtifactRevisionId,
    int ChunkNumber,
    string ContentHash,
    int SizeInBytes,
    int? SourceStart,
    int? SourceLength,
    ChunkingStrategyType Strategy,
    string StrategyVersion,
    DateTimeOffset CreatedAtUtc) : IDomainEvent;