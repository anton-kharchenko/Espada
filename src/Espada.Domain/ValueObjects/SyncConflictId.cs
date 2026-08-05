using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class SyncConflictId : ValueObject
    {
        private SyncConflictId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static SyncConflictId New()
        {
            return new SyncConflictId(Guid.NewGuid());
        }

        public static SyncConflictId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("SyncConflictId cannot be empty.", nameof(value))
                : new SyncConflictId(value);
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