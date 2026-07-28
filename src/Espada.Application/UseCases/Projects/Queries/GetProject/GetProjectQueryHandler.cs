using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.UseCases.Projects.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Projects.Queries.GetProject
{
    internal sealed class GetProjectQueryHandler(
        IProjectRepository projectRepository,
        IMapper mapper)
        : IQueryHandler<GetProjectQuery, ProjectResponse>
    {
        public async Task<DomainResult<ProjectResponse>> Handle(
            GetProjectQuery request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty || request.ProjectId == Guid.Empty)
            {
                DomainError error = request.WorkspaceId == Guid.Empty
                    ? WorkspaceApplicationErrors.InvalidId
                    : ProjectApplicationErrors.InvalidId;
                return DomainResult.Failure<ProjectResponse>(error);
            }

            Project? project = await projectRepository.GetByIdAsync(
                ProjectId.Create(request.ProjectId),
                cancellationToken);
            if (project is null)
            {
                return DomainResult.Failure<ProjectResponse>(
                    ProjectApplicationErrors.NotFound(request.ProjectId));
            }

            if (project.WorkspaceId.Value != request.WorkspaceId)
            {
                return DomainResult.Failure<ProjectResponse>(
                    ProjectApplicationErrors.NotFoundInWorkspace(
                        request.ProjectId,
                        request.WorkspaceId));
            }

            return DomainResult.Success(mapper.Map<ProjectResponse>(project));
        }
    }
}