namespace Espada.Application.UseCases.Context.Queries.SearchWorkspaceContext;

public sealed record WorkspaceContextItemResponse(
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