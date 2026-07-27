namespace Espada.Infrastructure.Options;

public sealed class EmbeddingModelOptions
{
    public string Identifier { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string ProviderModel { get; set; } = string.Empty;

    public int Dimensions { get; set; }
}