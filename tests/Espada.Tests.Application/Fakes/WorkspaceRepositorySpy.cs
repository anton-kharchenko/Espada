using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class WorkspaceRepositorySpy
        : IWorkspaceRepository
    {
        public Workspace? AddedWorkspace { get; private set; }

        public Workspace? WorkspaceToReturn { get; set; }

        public int AddCallCount { get; private set; }

        public int GetByIdCallCount { get; private set; }

        public WorkspaceId? ReceivedWorkspaceId { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public CancellationToken GetByIdCancellationToken { get; private set; }

        public Task AddAsync(Workspace workspace, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspace);

            AddedWorkspace = workspace;
            AddCallCount++;
            ReceivedCancellationToken = cancellationToken;

            return Task.CompletedTask;
        }

        public Task<Workspace?> GetByIdAsync(WorkspaceId workspaceId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);

            GetByIdCallCount++;
            ReceivedWorkspaceId = workspaceId;
            GetByIdCancellationToken = cancellationToken;

            return Task.FromResult(WorkspaceToReturn);
        }
    }
}