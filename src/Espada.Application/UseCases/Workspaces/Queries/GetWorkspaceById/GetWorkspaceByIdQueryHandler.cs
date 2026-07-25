using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Mappings;
using Espada.Application.UseCases.Workspaces.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Workspaces.Queries.GetWorkspaceById
{
    internal sealed class GetWorkspaceByIdQueryHandler(IWorkspaceRepository workspaceRepository) : IQueryHandler<GetWorkspaceByIdQuery, WorkspaceResponse>
    {
        public async Task<DomainResult<WorkspaceResponse>> Handle(GetWorkspaceByIdQuery request, CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure<WorkspaceResponse>(WorkspaceApplicationErrors.InvalidId);
            }

            WorkspaceId workspaceId = WorkspaceId.Create(request.WorkspaceId);

            Workspace? workspace = await workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);

            if (workspace is null)
            {
                return DomainResult.Failure<WorkspaceResponse>(WorkspaceApplicationErrors.NotFound(request.WorkspaceId));
            }

            WorkspaceResponse response = workspace.ToResponse();

            return DomainResult.Success(response);
        }
    }
}