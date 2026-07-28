using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.UseCases.Projects.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.ProjectTasks.Queries.ListProjectTasks
{
    internal sealed class ListProjectTasksQueryHandler(
        IProjectRepository projectRepository,
        IProjectTaskRepository projectTaskRepository,
        IMapper mapper)
        : IQueryHandler<ListProjectTasksQuery, ListProjectTasksResponse>
    {
        public async Task<DomainResult<ListProjectTasksResponse>> Handle(
            ListProjectTasksQuery request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty || request.ProjectId == Guid.Empty)
            {
                DomainError error = request.WorkspaceId == Guid.Empty
                    ? WorkspaceApplicationErrors.InvalidId
                    : ProjectApplicationErrors.InvalidId;
                return DomainResult.Failure<ListProjectTasksResponse>(error);
            }

            Project? project = await projectRepository.GetByIdAsync(
                ProjectId.Create(request.ProjectId),
                cancellationToken);
            if (project is null || project.WorkspaceId.Value != request.WorkspaceId)
            {
                DomainError error = project is null
                    ? ProjectApplicationErrors.NotFound(request.ProjectId)
                    : ProjectApplicationErrors.NotFoundInWorkspace(
                        request.ProjectId,
                        request.WorkspaceId);
                return DomainResult.Failure<ListProjectTasksResponse>(error);
            }

            IReadOnlyList<ProjectTask> tasks = await projectTaskRepository.ListByProjectIdAsync(
                project.WorkspaceId,
                project.Id,
                cancellationToken);
            ProjectTaskResponse[] items = mapper.Map<ProjectTaskResponse[]>(tasks);

            return DomainResult.Success(new ListProjectTasksResponse(items));
        }
    }
}