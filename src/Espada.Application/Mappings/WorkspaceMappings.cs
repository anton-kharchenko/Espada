using Espada.Application.UseCases.Workspaces.Common;
using Espada.Domain.Aggregates;

namespace Espada.Application.Mappings
{
    internal static class WorkspaceMappings
    {
        public static WorkspaceResponse ToResponse(this Workspace workspace)
        {
            ArgumentNullException.ThrowIfNull(workspace);

            return new WorkspaceResponse(workspace.Id.Value, workspace.Name.Value, workspace.Type.Id, workspace.Type.Name, workspace.Status.Id, workspace.Status.Name, workspace.CreatedAtUtc);
        }
    }
}