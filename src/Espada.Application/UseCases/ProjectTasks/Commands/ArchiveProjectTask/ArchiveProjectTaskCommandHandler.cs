using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.UseCases.Projects.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.ProjectTasks.Commands.ArchiveProjectTask
{
    internal sealed class ArchiveProjectTaskCommandHandler(
        IProjectTaskRepository projectTaskRepository,
        IUnitOfWork unitOfWork,
        IClockService clockService,
        IMapper mapper)
        : ICommandHandler<ArchiveProjectTaskCommand, ProjectTaskResponse>
    {
        public async Task<DomainResult<ProjectTaskResponse>> Handle(
            ArchiveProjectTaskCommand request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty || request.TaskId == Guid.Empty)
            {
                DomainError error = request.WorkspaceId == Guid.Empty
                    ? WorkspaceApplicationErrors.InvalidId
                    : ProjectTaskApplicationErrors.InvalidId;
                return DomainResult.Failure<ProjectTaskResponse>(error);
            }

            ProjectTask? task = await projectTaskRepository.GetByIdAsync(
                TaskId.Create(request.TaskId),
                cancellationToken);
            if (task is null || task.WorkspaceId.Value != request.WorkspaceId)
            {
                DomainError error = task is null
                    ? ProjectTaskApplicationErrors.NotFound(request.TaskId)
                    : ProjectTaskApplicationErrors.NotFoundInWorkspace(
                        request.TaskId,
                        request.WorkspaceId);
                return DomainResult.Failure<ProjectTaskResponse>(error);
            }

            DomainResult result = task.Archive(clockService.UtcNow);
            if (result.IsFailure)
            {
                return DomainResult.Failure<ProjectTaskResponse>(result.Error);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return DomainResult.Success(mapper.Map<ProjectTaskResponse>(task));
        }
    }
}