using Espada.Application.Contracts.Messaging;
using Espada.Application.UseCases.Projects.Common;

namespace Espada.Application.UseCases.ProjectTasks.Queries.GetProjectTask
{
    public sealed record GetProjectTaskQuery(
        Guid WorkspaceId,
        Guid TaskId) : IQuery<ProjectTaskResponse>;
}