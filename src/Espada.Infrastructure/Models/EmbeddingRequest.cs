using System.Text.Json.Serialization;

namespace Espada.Infrastructure.Models;

internal sealed record EmbeddingRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("input")] object Input);