using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories;

internal sealed class WorkspaceMembershipRepository(EspadaDbContext dbContext) : IWorkspaceMembershipRepository
{
    public async Task AddAsync(WorkspaceMembership membership, CancellationToken cancellationToken = default) =>
        await dbContext.WorkspaceMemberships.AddAsync(membership, cancellationToken);

    public Task<bool> IsMemberAsync(WorkspaceId workspaceId, string issuer, string subject, CancellationToken cancellationToken = default) =>
        dbContext.WorkspaceMemberships
            .AsNoTracking()
            .AnyAsync(membership => membership.WorkspaceId == workspaceId && membership.Issuer == issuer && membership.Subject == subject, cancellationToken);
}