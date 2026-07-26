using Espada.Domain.ValueObjects;

namespace Espada.Infrastructure.Models
{
    internal sealed class EmbeddingVectorRecord
    {
        private EmbeddingVectorRecord()
        {
        }

        public EmbeddingVectorRecord(ChunkEmbeddingId chunkEmbeddingId, IReadOnlyList<float> vector)
        {
            ArgumentNullException.ThrowIfNull(chunkEmbeddingId);
            ArgumentNullException.ThrowIfNull(vector);

            ChunkEmbeddingId = chunkEmbeddingId;
            Vector = vector.ToArray();
        }

        public ChunkEmbeddingId ChunkEmbeddingId { get; private set; } = null!;

        public float[] Vector { get; private set; } = [];
    }
}