namespace Espada.Infrastructure.Options;

public sealed class EmbeddingGenerationOptions
{
    public const string SectionName = "EmbeddingGeneration";

    public string BaseUrl { get; set; } = string.Empty;

    public string? ApiKey { get; set; }

    public List<EmbeddingModelOptions> Models { get; set; } = [];
}