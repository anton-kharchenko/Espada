using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class WorkspaceMembershipRepositorySpy : IWorkspaceMembershipRepository
    {
        public WorkspaceMembership? AddedMembership { get; private set; }

        public bool IsMember { get; set; }

        public int IsMemberCallCount { get; private set; }

        public WorkspaceId? ReceivedWorkspaceId { get; private set; }

        public string? ReceivedIssuer { get; private set; }

        public string? ReceivedSubject { get; private set; }

        public IReadOnlyList<Workspace> Workspaces { get; set; } = [];

        public int ListWorkspacesCallCount { get; private set; }

        public bool IsOwner { get; set; }

        public CancellationToken ListWorkspacesCancellationToken { get; private set; }

        public Task AddAsync(WorkspaceMembership membership, CancellationToken cancellationToken = default)
        {
            AddedMembership = membership;
            return Task.CompletedTask;
        }

        public Task<bool> IsMemberAsync(
            WorkspaceId workspaceId,
            string issuer,
            string subject,
            CancellationToken cancellationToken = default)
        {
            IsMemberCallCount++;
            ReceivedWorkspaceId = workspaceId;
            ReceivedIssuer = issuer;
            ReceivedSubject = subject;

            return Task.FromResult(IsMember);
        }

        public Task<IReadOnlyList<Workspace>> ListWorkspacesAsync(
            string issuer,
            string subject,
            CancellationToken cancellationToken = default)
        {
            ListWorkspacesCallCount++;
            ReceivedIssuer = issuer;
            ReceivedSubject = subject;
            ListWorkspacesCancellationToken = cancellationToken;

            return Task.FromResult(Workspaces);
        }

        public Task<bool> IsOwnerAsync(
            WorkspaceId workspaceId,
            string issuer,
            string subject,
            CancellationToken cancellationToken = default)
        {
            ReceivedWorkspaceId = workspaceId;
            ReceivedIssuer = issuer;
            ReceivedSubject = subject;

            return Task.FromResult(IsOwner);
        }
    }
}
