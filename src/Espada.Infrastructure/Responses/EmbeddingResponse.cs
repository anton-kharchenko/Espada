using Espada.Infrastructure.Models;
using System.Text.Json.Serialization;

namespace Espada.Infrastructure.Responses
{
    internal sealed record EmbeddingResponse(
        [property: JsonPropertyName("data")] IReadOnlyList<EmbeddingData>? Data,
        [property: JsonPropertyName("usage")] EmbeddingUsage? Usage);
}