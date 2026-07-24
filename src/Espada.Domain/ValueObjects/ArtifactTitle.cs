using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects;

public sealed class ArtifactTitle : ValueObject
{
    public const int MaxLength = 200;

    private ArtifactTitle(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static DomainResult<ArtifactTitle> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DomainResult<ArtifactTitle>.Failure(ArtifactErrors.TitleEmpty);
        }

        string normalized = value.Trim();

        return normalized.Length > MaxLength ? DomainResult<ArtifactTitle>.Failure(ArtifactErrors.TitleTooLong) : DomainResult<ArtifactTitle>.Success(new ArtifactTitle(normalized));
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}