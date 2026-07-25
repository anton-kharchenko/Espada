using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors;

public static class ChunkEmbeddingApplicationErrors
{
    public static readonly DomainError InvalidId = new("ChunkEmbedding.Id.Invalid", "Chunk embedding ID cannot be empty.");

    public static DomainError NotFoundForChunk(Guid chunkId) =>
        new("ChunkEmbedding.NotFoundForChunk", $"A chunk embedding for chunk '{chunkId:D}' was not found.");

    public static DomainError VectorNotFound(Guid chunkEmbeddingId) =>
        new("ChunkEmbedding.Vector.NotFound", $"The vector for chunk embedding '{chunkEmbeddingId:D}' was not found.");
}