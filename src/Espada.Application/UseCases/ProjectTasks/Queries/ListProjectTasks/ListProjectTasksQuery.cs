using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.ProjectTasks.Queries.ListProjectTasks
{
    public sealed record ListProjectTasksQuery(
        Guid WorkspaceId,
        Guid ProjectId) : IQuery<ListProjectTasksResponse>;
}