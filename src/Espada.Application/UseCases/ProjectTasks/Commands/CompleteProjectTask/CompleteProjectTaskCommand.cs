using Espada.Application.Contracts.Messaging;
using Espada.Application.UseCases.Projects.Common;

namespace Espada.Application.UseCases.ProjectTasks.Commands.CompleteProjectTask
{
    public sealed record CompleteProjectTaskCommand(
        Guid WorkspaceId,
        Guid TaskId) : ICommand<ProjectTaskResponse>;
}