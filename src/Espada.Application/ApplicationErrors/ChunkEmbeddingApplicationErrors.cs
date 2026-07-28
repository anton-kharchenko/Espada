using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors
{
    public static class ChunkEmbeddingApplicationErrors
    {
        public static readonly DomainError InvalidId = new("ChunkEmbedding.Id.Invalid",
            "Chunk embedding ID cannot be empty.");

        public static readonly DomainError VectorContainsNonFiniteValue =
            new("ChunkEmbedding.Vector.NonFinite", "Embedding vector values must be finite.");

        public static DomainError NotFoundForChunk(Guid chunkId)
        {
            return new DomainError("ChunkEmbedding.NotFoundForChunk",
                $"A chunk embedding for chunk '{chunkId:D}' was not found.");
        }

        public static DomainError VectorNotFound(Guid chunkEmbeddingId)
        {
            return new DomainError("ChunkEmbedding.Vector.NotFound",
                $"The vector for chunk embedding '{chunkEmbeddingId:D}' was not found.");
        }

        public static DomainError AlreadyExistsForModel(Guid chunkId, string modelIdentifier, string modelVersion)
        {
            return new DomainError("ChunkEmbedding.AlreadyExists",
                $"Chunk '{chunkId:D}' already has an embedding for model '{modelIdentifier}@{modelVersion}'.");
        }

        public static DomainError DimensionMismatch(string modelIdentifier, string modelVersion, int expected,
            int actual)
        {
            return new DomainError("ChunkEmbedding.Dimension.Conflict",
                $"Model '{modelIdentifier}@{modelVersion}' expects {expected} dimensions, but the vector has {actual}.");
        }

        public static DomainError InconsistentModelDimensions(string modelIdentifier, string modelVersion)
        {
            return new DomainError("ChunkEmbedding.Dimension.Conflict",
                $"Model '{modelIdentifier}@{modelVersion}' has inconsistent stored dimensions.");
        }
    }
}