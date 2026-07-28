using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class SourceId : ValueObject
    {
        private SourceId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static SourceId New()
        {
            return new SourceId(Guid.NewGuid());
        }

        public static SourceId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("Source ID cannot be empty.", nameof(value))
                : new SourceId(value);
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