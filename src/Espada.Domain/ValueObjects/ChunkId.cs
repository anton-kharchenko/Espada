using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class ChunkId : ValueObject
    {
        private ChunkId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static ChunkId New()
        {
            return new ChunkId(Guid.NewGuid());
        }

        public static ChunkId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("Chunk ID cannot be empty.", nameof(value))
                : new ChunkId(value);
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