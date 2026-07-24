using System.Text;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects;

public sealed class ChunkContent : ValueObject
{
    private ChunkContent(string value)
    {
        Value = value;
        SizeInBytes = Encoding.UTF8.GetByteCount(value);
        Hash = ContentHash.FromUtf8(value);
    }

    public string Value { get; }

    public int SizeInBytes { get; }

    public ContentHash Hash { get; }

    public int CharacterCount => Value.Length;

    public static DomainResult<ChunkContent> Create(string? value) => 
        string.IsNullOrWhiteSpace(value) ? DomainResult<ChunkContent>.Failure(ChunkErrors.ContentEmpty) : DomainResult<ChunkContent>.Success(new ChunkContent(value));

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}