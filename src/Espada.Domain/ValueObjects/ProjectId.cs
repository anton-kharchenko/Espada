using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class ProjectId : ValueObject
    {
        private ProjectId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static ProjectId New()
        {
            return new ProjectId(Guid.NewGuid());
        }

        public static ProjectId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("Project ID cannot be empty.", nameof(value))
                : new ProjectId(value);
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