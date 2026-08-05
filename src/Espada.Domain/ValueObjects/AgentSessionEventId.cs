using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class AgentSessionEventId : ValueObject
    {
        private AgentSessionEventId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static AgentSessionEventId New()
        {
            return new AgentSessionEventId(Guid.NewGuid());
        }

        public static AgentSessionEventId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("AgentSessionEventId cannot be empty.", nameof(value))
                : new AgentSessionEventId(value);
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