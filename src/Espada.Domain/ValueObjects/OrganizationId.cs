using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class OrganizationId : ValueObject
    {
        private OrganizationId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static OrganizationId New()
        {
            return new OrganizationId(Guid.NewGuid());
        }

        public static OrganizationId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("Organization ID cannot be empty.", nameof(value))
                : new OrganizationId(value);
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