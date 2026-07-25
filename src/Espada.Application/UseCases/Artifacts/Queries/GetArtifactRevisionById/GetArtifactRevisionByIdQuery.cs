using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Artifacts.Queries.GetArtifactRevisionById
{
    public sealed record GetArtifactRevisionByIdQuery(
        Guid WorkspaceId,
        Guid ArtifactId,
        Guid ArtifactRevisionId)
        : IQuery<GetArtifactRevisionByIdResponse>;
}