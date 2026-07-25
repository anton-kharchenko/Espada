using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Artifacts.Commands.AddArtifactRevision
{
    public sealed record AddArtifactRevisionCommand(
        Guid WorkspaceId,
        Guid ArtifactId,
        string Content) : ICommand<AddArtifactRevisionResponse>;
}