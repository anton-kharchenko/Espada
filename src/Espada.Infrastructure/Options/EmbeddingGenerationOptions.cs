namespace Espada.Infrastructure.Options
{
    public sealed class EmbeddingGenerationOptions
    {
        public string BaseUrl { get; set; } = string.Empty;

        public string? ApiKey { get; set; }

        public string? DefaultModel { get; set; }

        public List<EmbeddingModelOptions> Models { get; set; } = [];
    }
}