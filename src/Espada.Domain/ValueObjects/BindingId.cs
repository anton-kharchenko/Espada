using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class BindingId : ValueObject
    {
        private BindingId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static BindingId New()
        {
            return new BindingId(Guid.NewGuid());
        }

        public static BindingId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("Binding ID cannot be empty.", nameof(value))
                : new BindingId(value);
        }

        public override string ToString()
        {
            return Value.ToString("D");
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}