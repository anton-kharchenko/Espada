using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Events
{
    public sealed record ChunkEmbeddingCreatedDomainEvent(
        ChunkEmbeddingId ChunkEmbeddingId,
        WorkspaceId WorkspaceId,
        ChunkId ChunkId,
        string ChunkContentHash,
        string ModelIdentifier,
        string ModelVersion,
        int Dimensions,
        DateTimeOffset CreatedAtUtc) : IDomainEvent;
}