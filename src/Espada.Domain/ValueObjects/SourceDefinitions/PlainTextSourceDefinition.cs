using Espada.Domain.Enums;
using System.Security.Cryptography;
using System.Text;

namespace Espada.Domain.ValueObjects.SourceDefinitions;

public sealed record PlainTextSourceDefinition : SourceDefinition
{
    public PlainTextSourceDefinition(string title, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        Title = title;
        Content = content;
    }

    public string Title { get; init; }

    public string Content { get; init; }

    public override SourceType SourceType => SourceType.PlainText;

    public override string CanonicalLocator => $"text:{Hash(Content)}";

    private static string Hash(string value) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}