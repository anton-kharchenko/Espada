using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Artifacts.Queries.GetArtifactById
{
    public sealed record GetArtifactByIdQuery(
        Guid WorkspaceId,
        Guid ArtifactId) : IQuery<GetArtifactByIdResponse>;
}