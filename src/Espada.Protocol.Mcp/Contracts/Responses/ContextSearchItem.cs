namespace Espada.Protocol.Mcp.Contracts.Responses
{
    public sealed record ContextSearchItem(
        Guid ChunkId,
        Guid ArtifactId,
        Guid RevisionId,
        string Content,
        double Similarity,
        int? SourceSpanStart,
        int? SourceSpanLength,
        double KeywordScore,
        double RecencyScore,
        double ArtifactPriorityScore,
        double SourcePriorityScore,
        double Score);
}