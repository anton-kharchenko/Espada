using Espada.Application.UseCases.Projects.Common;

namespace Espada.Application.UseCases.ProjectTasks.Queries.ListWorkspaceTasks
{
    public sealed record ListWorkspaceTasksResponse(
        IReadOnlyList<ProjectTaskResponse> Items);
}
