using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Artifacts.Queries.ListArtifactRevisions
{
    public sealed record ListArtifactRevisionsQuery(
        Guid WorkspaceId,
        Guid ArtifactId)
        : IQuery<ListArtifactRevisionsResponse>;
}