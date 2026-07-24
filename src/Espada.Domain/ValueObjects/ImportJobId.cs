using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects;

public sealed class ImportJobId : ValueObject
{
    private ImportJobId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static ImportJobId New() => new(Guid.NewGuid());

    public static ImportJobId Create(Guid value) => value == Guid.Empty ? throw new ArgumentException("Import job ID cannot be empty.", nameof(value)) : new ImportJobId(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString("D");
}