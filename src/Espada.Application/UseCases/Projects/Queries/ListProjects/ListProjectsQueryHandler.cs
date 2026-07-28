using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.UseCases.Projects.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Projects.Queries.ListProjects
{
    internal sealed class ListProjectsQueryHandler(
        IWorkspaceRepository workspaceRepository,
        IProjectRepository projectRepository,
        IMapper mapper)
        : IQueryHandler<ListProjectsQuery, ListProjectsResponse>
    {
        public async Task<DomainResult<ListProjectsResponse>> Handle(
            ListProjectsQuery request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure<ListProjectsResponse>(WorkspaceApplicationErrors.InvalidId);
            }

            WorkspaceId workspaceId = WorkspaceId.Create(request.WorkspaceId);
            Workspace? workspace = await workspaceRepository.GetByIdAsync(
                workspaceId,
                cancellationToken);
            if (workspace is null)
            {
                return DomainResult.Failure<ListProjectsResponse>(
                    WorkspaceApplicationErrors.NotFound(request.WorkspaceId));
            }

            IReadOnlyList<Project> projects = await projectRepository.ListByWorkspaceIdAsync(
                workspaceId,
                cancellationToken);
            ProjectResponse[] items = mapper.Map<ProjectResponse[]>(projects);

            return DomainResult.Success(new ListProjectsResponse(items));
        }
    }
}