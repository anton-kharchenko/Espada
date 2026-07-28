using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class WorkspaceMembershipRepository(EspadaDbContext dbContext) : IWorkspaceMembershipRepository
    {
        public async Task AddAsync(WorkspaceMembership membership, CancellationToken cancellationToken = default)
        {
            await dbContext.WorkspaceMemberships.AddAsync(membership, cancellationToken);
        }

        public Task<bool> IsMemberAsync(WorkspaceId workspaceId, string issuer, string subject,
            CancellationToken cancellationToken = default)
        {
            return dbContext.WorkspaceMemberships
                .AsNoTracking()
                .AnyAsync(
                    membership => membership.WorkspaceId == workspaceId && membership.Issuer == issuer &&
                                  membership.Subject == subject, cancellationToken);
        }

        public async Task<IReadOnlyList<Workspace>> ListWorkspacesAsync(
            string issuer,
            string subject,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
            ArgumentException.ThrowIfNullOrWhiteSpace(subject);

            return await dbContext.WorkspaceMemberships
                .AsNoTracking()
                .Where(membership =>
                    membership.Issuer == issuer
                    && membership.Subject == subject)
                .Join(
                    dbContext.Workspaces.AsNoTracking(),
                    membership => membership.WorkspaceId,
                    workspace => workspace.Id,
                    (_, workspace) => workspace)
                .OrderBy(workspace => workspace.Name)
                .ThenBy(workspace => workspace.Id)
                .ToArrayAsync(cancellationToken);
        }

        public Task<bool> IsOwnerAsync(
            WorkspaceId workspaceId,
            string issuer,
            string subject,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);
            ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
            ArgumentException.ThrowIfNullOrWhiteSpace(subject);

            return dbContext.WorkspaceMemberships
                .AsNoTracking()
                .AnyAsync(
                    membership =>
                        membership.WorkspaceId == workspaceId
                        && membership.Issuer == issuer
                        && membership.Subject == subject
                        && membership.Role
                        == WorkspaceMembershipRoleType.Owner,
                    cancellationToken);
        }
    }
}
