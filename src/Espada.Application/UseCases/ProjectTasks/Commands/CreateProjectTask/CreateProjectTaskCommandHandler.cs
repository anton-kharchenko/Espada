using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.UseCases.Projects.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.ProjectTasks.Commands.CreateProjectTask
{
    internal sealed class CreateProjectTaskCommandHandler(
        IProjectRepository projectRepository,
        IProjectTaskRepository projectTaskRepository,
        IUnitOfWork unitOfWork,
        IClockService clockService,
        IMapper mapper)
        : ICommandHandler<CreateProjectTaskCommand, ProjectTaskResponse>
    {
        public async Task<DomainResult<ProjectTaskResponse>> Handle(
            CreateProjectTaskCommand request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty || request.ProjectId == Guid.Empty)
            {
                DomainError error = request.WorkspaceId == Guid.Empty
                    ? WorkspaceApplicationErrors.InvalidId
                    : ProjectApplicationErrors.InvalidId;
                return DomainResult.Failure<ProjectTaskResponse>(error);
            }

            Project? project = await projectRepository.GetByIdAsync(
                ProjectId.Create(request.ProjectId),
                cancellationToken);
            if (project is null)
            {
                return DomainResult.Failure<ProjectTaskResponse>(
                    ProjectApplicationErrors.NotFound(request.ProjectId));
            }

            if (project.WorkspaceId.Value != request.WorkspaceId)
            {
                return DomainResult.Failure<ProjectTaskResponse>(
                    ProjectApplicationErrors.NotFoundInWorkspace(
                        request.ProjectId,
                        request.WorkspaceId));
            }

            DomainResult<ProjectTask> taskResult = project.CreateTask(
                TaskId.Create(Guid.NewGuid()),
                request.Title,
                clockService.UtcNow);
            if (taskResult.IsFailure)
            {
                return DomainResult.Failure<ProjectTaskResponse>(taskResult.Error);
            }

            await projectTaskRepository.AddAsync(taskResult.Value, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return DomainResult.Success(mapper.Map<ProjectTaskResponse>(taskResult.Value));
        }
    }
}