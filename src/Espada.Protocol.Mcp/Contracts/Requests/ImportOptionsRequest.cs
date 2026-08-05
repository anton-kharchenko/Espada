namespace Espada.Protocol.Mcp.Contracts.Requests
{
    public sealed record ImportOptionsRequest(
        string? EmbeddingModel = null,
        string ChunkingStrategy = "Recursive",
        int MaxCharacters = 2000,
        int OverlapCharacters = 200,
        double SemanticThreshold = 0.75,
        IReadOnlyList<string>? Separators = null,
        string? CodeLanguage = null);
}