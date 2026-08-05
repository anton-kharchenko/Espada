using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class SyncEventId : ValueObject
    {
        private SyncEventId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static SyncEventId New()
        {
            return new SyncEventId(Guid.NewGuid());
        }

        public static SyncEventId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("SyncEventId cannot be empty.", nameof(value))
                : new SyncEventId(value);
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