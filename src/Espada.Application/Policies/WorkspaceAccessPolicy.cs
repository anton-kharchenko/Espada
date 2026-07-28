using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Security;
using Espada.Application.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Espada.Application.Constants;

namespace Espada.Application.Policies
{
    public sealed class WorkspaceAccessPolicy(
        IRequestPrincipalAccessor principalAccessor,
        IWorkspaceMembershipRepository workspaceMembershipRepository,
        IOrganizationMembershipRepository organizationMembershipRepository,
        IWorkspaceRepository workspaceRepository)
    {
        public DomainResult AuthorizeWorkspaceCreation()
        {
            RequestPrincipal? principal = principalAccessor.Principal;
            if (principal is null || !IsAuthenticated(principal))
            {
                return DomainResult.Failure(AccessPolicyErrors.Unauthorized);
            }

            if (principal.WorkspaceId.HasValue)
            {
                return DomainResult.Failure(AccessPolicyErrors.WorkspaceMismatch);
            }

            return principal.HasScope(ApplicationScopeConstants.WorkspaceCreate)
                ? DomainResult.Success()
                : DomainResult.Failure(
                    AccessPolicyErrors.MissingScope(
                        ApplicationScopeConstants.WorkspaceCreate));
        }

        public async Task<DomainResult> AuthorizeWorkspaceAsync(
            Guid workspaceId,
            string requiredScope,
            CancellationToken cancellationToken)
        {
            if (workspaceId == Guid.Empty)
            {
                return DomainResult.Failure(WorkspaceApplicationErrors.InvalidId);
            }

            RequestPrincipal? principal = principalAccessor.Principal;
            if (principal is null || !IsAuthenticated(principal))
            {
                return DomainResult.Failure(AccessPolicyErrors.Unauthorized);
            }

            if (!principal.HasScope(requiredScope))
            {
                return DomainResult.Failure(
                    AccessPolicyErrors.MissingScope(requiredScope));
            }

            if (principal.WorkspaceId != workspaceId)
            {
                return DomainResult.Failure(AccessPolicyErrors.WorkspaceMismatch);
            }

            return await AuthorizeWorkspaceGrantAsync(
                workspaceId,
                principal.IdentityIssuer,
                principal.IdentitySubject,
                cancellationToken);
        }

        public async Task<DomainResult> AuthorizeWorkspaceGrantAsync(
            Guid workspaceId,
            string identityIssuer,
            string identitySubject,
            CancellationToken cancellationToken)
        {
            if (workspaceId == Guid.Empty)
            {
                return DomainResult.Failure(WorkspaceApplicationErrors.InvalidId);
            }

            if (string.IsNullOrWhiteSpace(identityIssuer)
                || string.IsNullOrWhiteSpace(identitySubject))
            {
                return DomainResult.Failure(AccessPolicyErrors.Unauthorized);
            }

            WorkspaceId requestedWorkspaceId = WorkspaceId.Create(workspaceId);
            bool isWorkspaceMember =
                await workspaceMembershipRepository.IsMemberAsync(
                    requestedWorkspaceId,
                    identityIssuer,
                    identitySubject,
                    cancellationToken);
            if (!isWorkspaceMember)
            {
                return DomainResult.Failure(
                    AccessPolicyErrors.WorkspaceMembershipRequired);
            }

            Workspace? workspace = await workspaceRepository.GetByIdAsync(
                requestedWorkspaceId,
                cancellationToken);
            if (workspace is null)
            {
                return DomainResult.Failure(
                    WorkspaceApplicationErrors.NotFound(workspaceId));
            }

            if (workspace.OrganizationId is null)
            {
                return DomainResult.Success();
            }

            OrganizationMembership? organizationMembership =
                await organizationMembershipRepository.GetByIdentityAsync(
                    workspace.OrganizationId,
                    identityIssuer,
                    identitySubject,
                    cancellationToken);

            return organizationMembership is null
                ? DomainResult.Failure(
                    AccessPolicyErrors.OrganizationMembershipRequired)
                : DomainResult.Success();
        }

        private static bool IsAuthenticated(RequestPrincipal principal)
        {
            return !string.IsNullOrWhiteSpace(principal.IdentityIssuer)
                   && !string.IsNullOrWhiteSpace(principal.IdentitySubject)
                   && !string.IsNullOrWhiteSpace(principal.ClientId);
        }
    }
}