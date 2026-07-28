using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class OrganizationMembershipId : ValueObject
    {
        private OrganizationMembershipId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static OrganizationMembershipId New()
        {
            return new OrganizationMembershipId(Guid.NewGuid());
        }

        public static OrganizationMembershipId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("Organization membership ID cannot be empty.", nameof(value))
                : new OrganizationMembershipId(value);
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