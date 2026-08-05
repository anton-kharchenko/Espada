using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class SyncCursorId : ValueObject
    {
        private SyncCursorId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static SyncCursorId New()
        {
            return new SyncCursorId(Guid.NewGuid());
        }

        public static SyncCursorId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("SyncCursorId cannot be empty.", nameof(value))
                : new SyncCursorId(value);
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