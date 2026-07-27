using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes;

internal sealed class WorkspaceMembershipRepositorySpy : IWorkspaceMembershipRepository
{
    public WorkspaceMembership? AddedMembership { get; private set; }

    public Task AddAsync(WorkspaceMembership membership, CancellationToken cancellationToken = default)
    {
        AddedMembership = membership;
        return Task.CompletedTask;
    }

    public Task<bool> IsMemberAsync(WorkspaceId workspaceId, string issuer, string subject, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);
}