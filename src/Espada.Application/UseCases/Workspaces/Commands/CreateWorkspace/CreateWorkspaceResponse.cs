namespace Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace
{
    public sealed record CreateWorkspaceResponse(
        Guid WorkspaceId,
        Guid? OrganizationId);
}