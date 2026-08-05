using Espada.Application.Contracts.Messaging;
using Espada.Application.UseCases.Projects.Common;

namespace Espada.Application.UseCases.ProjectTasks.Commands.CreateProjectTask
{
    public sealed record CreateProjectTaskCommand(
        Guid WorkspaceId,
        Guid ProjectId,
        string Title) : ICommand<ProjectTaskResponse>;
}