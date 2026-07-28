using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Artifacts.Queries.ListArtifacts
{
    public sealed record ListArtifactsQuery(
        Guid WorkspaceId,
        int? KindTypeId = null) : IQuery<ListArtifactsResponse>;
}