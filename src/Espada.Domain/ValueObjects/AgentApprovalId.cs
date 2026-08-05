using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class AgentApprovalId : ValueObject
    {
        private AgentApprovalId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static AgentApprovalId New()
        {
            return new AgentApprovalId(Guid.NewGuid());
        }

        public static AgentApprovalId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("AgentApprovalId cannot be empty.", nameof(value))
                : new AgentApprovalId(value);
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