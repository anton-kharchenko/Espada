using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class AgentSessionId : ValueObject
    {
        private AgentSessionId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static AgentSessionId New()
        {
            return new AgentSessionId(Guid.NewGuid());
        }

        public static AgentSessionId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("AgentSessionId cannot be empty.", nameof(value))
                : new AgentSessionId(value);
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
