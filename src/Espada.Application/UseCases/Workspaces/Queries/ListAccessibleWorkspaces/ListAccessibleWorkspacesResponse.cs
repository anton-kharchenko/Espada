using Espada.Application.UseCases.Workspaces.Common;

namespace Espada.Application.UseCases.Workspaces.Queries.ListAccessibleWorkspaces
{
    public sealed record ListAccessibleWorkspacesResponse(
        IReadOnlyList<WorkspaceResponse> Items);
}