using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects;

public sealed class ChunkingVersion : ValueObject
{
    public const int MaxLength = 64;

    private ChunkingVersion(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static DomainResult<ChunkingVersion> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DomainResult<ChunkingVersion>.Failure(ChunkErrors.VersionEmpty);
        }

        string normalized = value.Trim();

        return normalized.Length > MaxLength ? DomainResult<ChunkingVersion>.Failure(ChunkErrors.VersionTooLong) : DomainResult<ChunkingVersion>.Success(new ChunkingVersion(normalized));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}