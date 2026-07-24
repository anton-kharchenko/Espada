using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;

namespace Espada.Tests.Application.Fakes;

internal sealed class WorkspaceRepositorySpy : IWorkspaceRepository
{
    public Workspace? AddedWorkspace { get; private set; }

    public int AddCallCount { get; private set; }

    public CancellationToken ReceivedCancellationToken { get; private set; }

    public Task AddAsync(Workspace workspace, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        AddedWorkspace = workspace;
        AddCallCount++;
        ReceivedCancellationToken = cancellationToken;

        return Task.CompletedTask;
    }
}