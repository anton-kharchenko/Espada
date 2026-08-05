using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.UseCases.Workspaces.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;

namespace Espada.Application.UseCases.Workspaces.Queries.ListAccessibleWorkspaces
{
    internal sealed class ListAccessibleWorkspacesQueryHandler(
        IWorkspaceMembershipRepository workspaceMembershipRepository,
        IMapper mapper)
        : IQueryHandler<ListAccessibleWorkspacesQuery, ListAccessibleWorkspacesResponse>
    {
        public async Task<DomainResult<ListAccessibleWorkspacesResponse>> Handle(
            ListAccessibleWorkspacesQuery request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.IdentityIssuer)
                || string.IsNullOrWhiteSpace(request.IdentitySubject))
            {
                return DomainResult.Failure<ListAccessibleWorkspacesResponse>(
                    AccessPolicyErrors.Unauthorized);
            }

            IReadOnlyList<Workspace> workspaces =
                await workspaceMembershipRepository.ListWorkspacesAsync(
                    request.IdentityIssuer.Trim(),
                    request.IdentitySubject.Trim(),
                    cancellationToken);
            WorkspaceResponse[] items =
                mapper.Map<WorkspaceResponse[]>(workspaces);

            return DomainResult.Success(
                new ListAccessibleWorkspacesResponse(items));
        }
    }
}