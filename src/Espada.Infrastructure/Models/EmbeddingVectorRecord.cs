using Espada.Domain.ValueObjects;
using Pgvector;

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
            Vector = new Vector(vector.ToArray());
        }

        public ChunkEmbeddingId ChunkEmbeddingId { get; private set; } = null!;

        public Vector Vector { get; private set; } = new(Array.Empty<float>());

        public void Replace(IReadOnlyList<float> vector)
        {
            ArgumentNullException.ThrowIfNull(vector);
            Vector = new Vector(vector.ToArray());
        }
    }
}