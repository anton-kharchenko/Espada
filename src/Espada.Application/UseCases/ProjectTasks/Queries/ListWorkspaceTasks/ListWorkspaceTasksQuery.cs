using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.ProjectTasks.Queries.ListWorkspaceTasks
{
    public sealed record ListWorkspaceTasksQuery(
        Guid WorkspaceId) : IQuery<ListWorkspaceTasksResponse>;
}