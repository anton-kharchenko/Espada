namespace Espada.Infrastructure.Models
{
    internal sealed record SearchCandidate(
        Guid ChunkId,
        Guid ArtifactId,
        Guid RevisionId,
        string Content,
        int? SourceSpanStart,
        int? SourceSpanLength,
        DateTimeOffset CreatedAtUtc,
        double KeywordScore,
        int ArtifactPriority,
        int SourcePriority);
}