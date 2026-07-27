using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence;

public interface IWorkspaceMembershipRepository
{
    Task AddAsync(
        WorkspaceMembership membership,
        CancellationToken cancellationToken = default);

    Task<bool> IsMemberAsync(
        WorkspaceId workspaceId,
        string issuer,
        string subject,
        CancellationToken cancellationToken = default);
}