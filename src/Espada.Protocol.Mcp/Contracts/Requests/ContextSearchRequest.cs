namespace Espada.Protocol.Mcp.Contracts.Requests;

public sealed record ContextSearchRequest(
    Guid WorkspaceId,
    string QueryText,
    string ModelIdentifier,
    string ModelVersion,
    int TopK = 10,
    IReadOnlyList<Guid>? ArtifactIds = null,
    IReadOnlyList<Guid>? RevisionIds = null,
    IReadOnlyList<Guid>? SourceIds = null,
    IReadOnlyList<int>? ArtifactTypeIds = null,
    IReadOnlyList<int>? SourceTypeIds = null,
    DateTimeOffset? CreatedAfterUtc = null,
    double? MinimumSimilarity = null,
    int? MinimumArtifactPriority = null,
    int? MinimumSourcePriority = null);