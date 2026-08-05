using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.UseCases.Projects.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.ProjectTasks.Queries.ListWorkspaceTasks
{
    internal sealed class ListWorkspaceTasksQueryHandler(
        IWorkspaceRepository workspaceRepository,
        IProjectTaskRepository projectTaskRepository,
        IMapper mapper)
        : IQueryHandler<ListWorkspaceTasksQuery, ListWorkspaceTasksResponse>
    {
        public async Task<DomainResult<ListWorkspaceTasksResponse>> Handle(
            ListWorkspaceTasksQuery request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure<ListWorkspaceTasksResponse>(
                    WorkspaceApplicationErrors.InvalidId);
            }

            WorkspaceId workspaceId =
                WorkspaceId.Create(request.WorkspaceId);
            Workspace? workspace = await workspaceRepository.GetByIdAsync(
                workspaceId,
                cancellationToken);
            if (workspace is null)
            {
                return DomainResult.Failure<ListWorkspaceTasksResponse>(
                    WorkspaceApplicationErrors.NotFound(
                        request.WorkspaceId));
            }

            IReadOnlyList<ProjectTask> tasks =
                await projectTaskRepository.ListByWorkspaceIdAsync(
                    workspaceId,
                    cancellationToken);
            ProjectTaskResponse[] items =
                mapper.Map<ProjectTaskResponse[]>(tasks);

            return DomainResult.Success(
                new ListWorkspaceTasksResponse(items));
        }
    }
}