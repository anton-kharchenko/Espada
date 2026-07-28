using Espada.Application.Contracts.Messaging;
using Espada.Domain.Enums;

namespace Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace
{
    public sealed record CreateWorkspaceCommand(
        string Name,
        WorkspaceType Type,
        Guid? OrganizationId = null,
        string? IdentityIssuer = null,
        string? IdentitySubject = null) : ICommand<CreateWorkspaceResponse>;
}