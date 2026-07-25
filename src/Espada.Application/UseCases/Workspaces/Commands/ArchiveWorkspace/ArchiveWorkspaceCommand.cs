using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Workspaces.Commands.ArchiveWorkspace
{
    public sealed record ArchiveWorkspaceCommand(Guid WorkspaceId) : ICommand;
}