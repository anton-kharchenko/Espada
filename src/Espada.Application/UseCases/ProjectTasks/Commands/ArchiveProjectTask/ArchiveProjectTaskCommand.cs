using Espada.Application.Contracts.Messaging;
using Espada.Application.UseCases.Projects.Common;

namespace Espada.Application.UseCases.ProjectTasks.Commands.ArchiveProjectTask
{
    public sealed record ArchiveProjectTaskCommand(
        Guid WorkspaceId,
        Guid TaskId) : ICommand<ProjectTaskResponse>;
}