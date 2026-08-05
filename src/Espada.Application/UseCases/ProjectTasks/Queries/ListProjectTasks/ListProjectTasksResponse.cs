using Espada.Application.UseCases.Projects.Common;

namespace Espada.Application.UseCases.ProjectTasks.Queries.ListProjectTasks
{
    public sealed record ListProjectTasksResponse(
        IReadOnlyList<ProjectTaskResponse> Items);
}