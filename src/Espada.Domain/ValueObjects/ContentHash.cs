using Espada.Domain.SeedWork;
using System.Security.Cryptography;
using System.Text;

namespace Espada.Domain.ValueObjects;

public sealed class ContentHash : ValueObject
{
    private ContentHash(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ContentHash FromUtf8(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        byte[] bytes = Encoding.UTF8.GetBytes(content);
        byte[] hash = SHA256.HashData(bytes);

        return new ContentHash(Convert.ToHexString(hash).ToLowerInvariant());
    }

    public static ContentHash Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new ContentHash(value.ToLowerInvariant());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}