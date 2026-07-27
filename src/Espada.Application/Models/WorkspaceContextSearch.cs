namespace Espada.Application.Models;

public sealed record WorkspaceContextSearch(
    Guid WorkspaceId,
    string QueryText,
    IReadOnlyList<float> QueryVector,
    string ModelIdentifier,
    string ModelVersion,
    int TopK,
    IReadOnlyCollection<Guid> ArtifactIds,
    IReadOnlyCollection<Guid> RevisionIds,
    IReadOnlyCollection<Guid> SourceIds,
    IReadOnlyCollection<int> ArtifactTypeIds,
    IReadOnlyCollection<int> SourceTypeIds,
    DateTimeOffset? CreatedAfterUtc,
    double? MinimumSimilarity,
    int? MinimumArtifactPriority,
    int? MinimumSourcePriority,
    DateTimeOffset NowUtc);