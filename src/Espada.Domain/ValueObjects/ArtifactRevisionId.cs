using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class ArtifactRevisionId : ValueObject
    {
        private ArtifactRevisionId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static ArtifactRevisionId New()
        {
            return new ArtifactRevisionId(Guid.NewGuid());
        }

        public static ArtifactRevisionId Create(Guid value)
        {
            return value == Guid.Empty
                ? throw new ArgumentException("Artifact revision ID cannot be empty.", nameof(value))
                : new ArtifactRevisionId(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return Value.ToString("D");
        }
    }
}