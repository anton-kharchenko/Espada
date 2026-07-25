using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Artifacts.Commands.CreateArtifact
{
    public sealed record CreateArtifactCommand(
        Guid WorkspaceId,
        string Title,
        int TypeId,
        string Content) : ICommand<CreateArtifactResponse>;
}