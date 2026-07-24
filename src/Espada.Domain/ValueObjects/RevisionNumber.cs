using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects;

public sealed class RevisionNumber : ValueObject
{
    private RevisionNumber(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static RevisionNumber First() => new(1);

    public static DomainResult<RevisionNumber> Create(int value) =>
        value < 1 ? DomainResult<RevisionNumber>.Failure(ArtifactRevisionErrors.InvalidRevisionNumber) : DomainResult<RevisionNumber>.Success(new RevisionNumber(value));

    public RevisionNumber Next() => new(checked(Value + 1));

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}