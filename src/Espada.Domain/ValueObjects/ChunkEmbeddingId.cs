using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class ChunkEmbeddingId : ValueObject
    {
        private ChunkEmbeddingId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static ChunkEmbeddingId New()
        {
            return new ChunkEmbeddingId(Guid.NewGuid());
        }

        public static ChunkEmbeddingId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("Chunk embedding ID cannot be empty.", nameof(value))
                : new ChunkEmbeddingId(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return Value.ToString("D");
        }
    }
}