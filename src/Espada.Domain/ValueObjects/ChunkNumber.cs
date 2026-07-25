using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects;

public sealed class ChunkNumber : ValueObject
{
    private ChunkNumber(int value) => Value = value;

    public int Value { get; }

    public static ChunkNumber First() => new(1);

    public static DomainResult<ChunkNumber> Create(int value) =>
        value < 1 ? DomainResult<ChunkNumber>.Failure(ChunkErrors.InvalidNumber) : DomainResult<ChunkNumber>.Success(new ChunkNumber(value));

    public ChunkNumber Next() => new(checked(Value + 1));

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}