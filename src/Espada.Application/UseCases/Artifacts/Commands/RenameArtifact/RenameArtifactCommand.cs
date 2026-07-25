using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Artifacts.Commands.RenameArtifact
{
    public sealed record RenameArtifactCommand(
        Guid WorkspaceId,
        Guid ArtifactId,
        string Title) : ICommand;
}