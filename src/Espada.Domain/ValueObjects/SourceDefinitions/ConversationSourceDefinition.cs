using Espada.Domain.Constants;
using Espada.Domain.Enums;
using System.Security.Cryptography;
using System.Text.Json;

namespace Espada.Domain.ValueObjects.SourceDefinitions;

public sealed record ConversationSourceDefinition : SourceDefinition
{
    public ConversationSourceDefinition(string title, IReadOnlyList<ConversationMessage> messages)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(messages);
        if (messages.Count is 0 or > ConversationSourceDefinitionConstants.MaximumMessages)
        {
            throw new ArgumentOutOfRangeException(nameof(messages), $"Conversation must contain between 1 and {ConversationSourceDefinitionConstants.MaximumMessages} messages.");
        }

        Title = title;
        Messages = messages;
    }

    public string Title { get; init; }

    public IReadOnlyList<ConversationMessage> Messages { get; init; }

    public override SourceType SourceType => SourceType.Conversation;

    public override string CanonicalLocator => $"conversation:{Hash(Messages)}";

    private static string Hash(IReadOnlyList<ConversationMessage> messages)
    {
        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(messages);
        return Convert.ToHexStringLower(SHA256.HashData(payload));
    }
}