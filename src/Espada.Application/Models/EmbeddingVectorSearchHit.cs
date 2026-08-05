using Espada.Domain.ValueObjects;

namespace Espada.Application.Models
{
    public sealed record EmbeddingVectorSearchHit(
        ChunkEmbeddingId ChunkEmbeddingId,
        ChunkId ChunkId,
        double Similarity);
}