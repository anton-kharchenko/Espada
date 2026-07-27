using System.Text.Json.Serialization;

namespace Espada.Infrastructure.Models;

internal sealed record EmbeddingResponse(
    [property: JsonPropertyName("data")]
    IReadOnlyList<EmbeddingData>? Data,
    [property: JsonPropertyName("usage")]
    EmbeddingUsage? Usage);