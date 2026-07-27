using System.Text.Json.Serialization;

namespace Espada.Infrastructure.Models;

internal sealed record EmbeddingData([property: JsonPropertyName("embedding")] float[] Embedding);