using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class AgentProfileId : ValueObject
    {
        private AgentProfileId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static AgentProfileId New()
        {
            return new AgentProfileId(Guid.NewGuid());
        }

        public static AgentProfileId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("AgentProfileId cannot be empty.", nameof(value))
                : new AgentProfileId(value);
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