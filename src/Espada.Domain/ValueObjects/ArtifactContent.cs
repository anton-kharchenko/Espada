using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using System.Text;

namespace Espada.Domain.ValueObjects;

public sealed class ArtifactContent : ValueObject
{
    private ArtifactContent(string value)
    {
        Value = value;
        SizeInBytes = Encoding.UTF8.GetByteCount(value);
        Hash = ContentHash.FromUtf8(value);
    }

    public string Value { get; }

    public int SizeInBytes { get; }

    public ContentHash Hash { get; }

    public static DomainResult<ArtifactContent> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DomainResult<ArtifactContent>.Failure(ArtifactRevisionErrors.ContentEmpty);
        }

        return DomainResult<ArtifactContent>.Success(new ArtifactContent(value));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}