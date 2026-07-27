using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Context.Queries.SearchWorkspaceContext;

public sealed record SearchWorkspaceContextQuery(
    Guid WorkspaceId,
    string QueryText,
    IReadOnlyList<float> QueryVector,
    string ModelIdentifier,
    string ModelVersion,
    int TopK,
    IReadOnlyCollection<Guid>? ArtifactIds = null,
    IReadOnlyCollection<Guid>? RevisionIds = null,
    IReadOnlyCollection<Guid>? SourceIds = null,
    IReadOnlyCollection<int>? ArtifactTypeIds = null,
    IReadOnlyCollection<int>? SourceTypeIds = null,
    DateTimeOffset? CreatedAfterUtc = null,
    double? MinimumSimilarity = null,
    int? MinimumArtifactPriority = null,
    int? MinimumSourcePriority = null) : IQuery<SearchWorkspaceContextResponse>;