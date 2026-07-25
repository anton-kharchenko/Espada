using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Artifacts.Commands.ArchiveArtifact
{
    public sealed record ArchiveArtifactCommand(
        Guid WorkspaceId,
        Guid ArtifactId) : ICommand;
}