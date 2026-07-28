using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class TaskId : ValueObject
    {
        private TaskId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static TaskId New()
        {
            return new TaskId(Guid.NewGuid());
        }

        public static TaskId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("Task ID cannot be empty.", nameof(value))
                : new TaskId(value);
        }

        public override string ToString()
        {
            return Value.ToString("D");
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}