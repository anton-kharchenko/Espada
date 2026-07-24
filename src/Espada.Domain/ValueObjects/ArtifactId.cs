using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects;

public sealed class ArtifactId : ValueObject
{
    private ArtifactId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static ArtifactId New() => new(Guid.NewGuid());

    public static ArtifactId Create(Guid value) => value == Guid.Empty ? throw new ArgumentException("Artifact ID cannot be empty.", nameof(value)) : new ArtifactId(value);

    public override string ToString() => Value.ToString("D");

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}