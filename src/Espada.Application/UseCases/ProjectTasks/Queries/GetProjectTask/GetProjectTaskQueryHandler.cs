using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.UseCases.Projects.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.ProjectTasks.Queries.GetProjectTask
{
    internal sealed class GetProjectTaskQueryHandler(
        IProjectTaskRepository projectTaskRepository,
        IMapper mapper)
        : IQueryHandler<GetProjectTaskQuery, ProjectTaskResponse>
    {
        public async Task<DomainResult<ProjectTaskResponse>> Handle(
            GetProjectTaskQuery request,
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
            if (task is null)
            {
                return DomainResult.Failure<ProjectTaskResponse>(
                    ProjectTaskApplicationErrors.NotFound(request.TaskId));
            }

            if (task.WorkspaceId.Value != request.WorkspaceId)
            {
                return DomainResult.Failure<ProjectTaskResponse>(
                    ProjectTaskApplicationErrors.NotFoundInWorkspace(
                        request.TaskId,
                        request.WorkspaceId));
            }

            return DomainResult.Success(mapper.Map<ProjectTaskResponse>(task));
        }
    }
}