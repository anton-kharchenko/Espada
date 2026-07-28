using System.Text.Json.Serialization;

namespace Espada.Infrastructure.Models
{
    internal sealed record EmbeddingUsage(
        [property: JsonPropertyName("prompt_tokens")]
        long PromptTokens);
}