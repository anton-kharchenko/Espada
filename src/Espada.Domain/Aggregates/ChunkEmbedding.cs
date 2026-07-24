using Espada.Domain.Events;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class ChunkEmbedding : AggregateRoot<ChunkEmbeddingId>
    {
        private ChunkEmbedding() { }

        private ChunkEmbedding(
            ChunkEmbeddingId id,
            WorkspaceId workspaceId,
            ChunkId chunkId,
            ContentHash chunkContentHash,
            EmbeddingModel model,
            EmbeddingDimensions dimensions,
            DateTimeOffset createdAtUtc)
            : base(id)
        {
            WorkspaceId = workspaceId;
            ChunkId = chunkId;
            ChunkContentHash = chunkContentHash;
            Model = model;
            Dimensions = dimensions;
            CreatedAtUtc = createdAtUtc;
        }

        public WorkspaceId WorkspaceId { get; private set; } = null!;

        public ChunkId ChunkId { get; private set; } = null!;

        public ContentHash ChunkContentHash { get; private set; } = null!;

        public EmbeddingModel Model { get; private set; } = null!;

        public EmbeddingDimensions Dimensions { get; private set; } = null!;

        public DateTimeOffset CreatedAtUtc { get; private set; }

        public static DomainResult<ChunkEmbedding> Create(
            ChunkEmbeddingId id,
            WorkspaceId workspaceId,
            ChunkId chunkId,
            ContentHash chunkContentHash,
            EmbeddingModel model,
            EmbeddingDimensions dimensions,
            DateTimeOffset createdAtUtc)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(workspaceId);
            ArgumentNullException.ThrowIfNull(chunkId);
            ArgumentNullException.ThrowIfNull(chunkContentHash);
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(dimensions);

            ChunkEmbedding embedding = new(id, workspaceId, chunkId, chunkContentHash, model, dimensions, createdAtUtc);

            embedding.RaiseDomainEvent(new ChunkEmbeddingCreatedDomainEvent(embedding.Id, embedding.WorkspaceId, embedding.ChunkId, embedding.ChunkContentHash.Value, embedding.Model.Identifier, embedding.Model.Version, embedding.Dimensions.Value, embedding.CreatedAtUtc));

            return DomainResult<ChunkEmbedding>.Success(embedding);
        }
    }
}