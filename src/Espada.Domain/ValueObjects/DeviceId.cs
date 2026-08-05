using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class DeviceId : ValueObject
    {
        private DeviceId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static DeviceId New()
        {
            return new DeviceId(Guid.NewGuid());
        }

        public static DeviceId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("DeviceId cannot be empty.", nameof(value))
                : new DeviceId(value);
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