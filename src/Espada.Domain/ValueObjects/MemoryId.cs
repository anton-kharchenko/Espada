using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class MemoryId : ValueObject
    {
        private MemoryId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static MemoryId New()
        {
            return new MemoryId(Guid.NewGuid());
        }

        public static MemoryId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("Memory ID cannot be empty.", nameof(value))
                : new MemoryId(value);
        }

        public override string ToString()
        {
            return Value.ToString("D");
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    
        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}