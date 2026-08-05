using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class AgentInstallationId : ValueObject
    {
        private AgentInstallationId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static AgentInstallationId New()
        {
            return new AgentInstallationId(Guid.NewGuid());
        }

        public static AgentInstallationId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("AgentInstallationId cannot be empty.", nameof(value))
                : new AgentInstallationId(value);
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
