using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class ChunkBatchId : ValueObject
    {
        private ChunkBatchId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static ChunkBatchId New()
        {
            return new ChunkBatchId(Guid.NewGuid());
        }

        public static ChunkBatchId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("Chunk batch ID cannot be empty.", nameof(value))
                : new ChunkBatchId(value);
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