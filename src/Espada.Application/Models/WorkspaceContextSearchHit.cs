namespace Espada.Application.Models;

public sealed record WorkspaceContextSearchHit(
    Guid ChunkId,
    Guid ArtifactId,
    Guid RevisionId,
    string Content,
    int? SourceSpanStart,
    int? SourceSpanLength,
    double Similarity,
    double KeywordScore,
    double RecencyScore,
    double ArtifactPriorityScore,
    double SourcePriorityScore,
    double Score);