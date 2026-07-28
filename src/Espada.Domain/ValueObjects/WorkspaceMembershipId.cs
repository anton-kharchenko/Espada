using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class WorkspaceMembershipId : ValueObject
    {
        private WorkspaceMembershipId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static WorkspaceMembershipId New()
        {
            return new WorkspaceMembershipId(Guid.NewGuid());
        }

        public static WorkspaceMembershipId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("Workspace membership ID cannot be empty.", nameof(value))
                : new WorkspaceMembershipId(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}