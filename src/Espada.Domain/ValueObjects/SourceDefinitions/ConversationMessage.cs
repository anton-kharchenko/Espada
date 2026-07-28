namespace Espada.Domain.ValueObjects.SourceDefinitions
{
    public sealed record ConversationMessage
    {
        public ConversationMessage(string role, string? author, string content, DateTimeOffset? timestamp)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(role);
            ArgumentException.ThrowIfNullOrWhiteSpace(content);

            Role = role;
            Author = author;
            Content = content;
            Timestamp = timestamp;
        }

        public string Role { get; init; }

        public string? Author { get; init; }

        public string Content { get; init; }

        public DateTimeOffset? Timestamp { get; init; }
    }
}