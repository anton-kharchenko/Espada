using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class WorkspaceContextSearchStoreSpy : IWorkspaceContextSearchStore
    {
        public WorkspaceContextSearch? ReceivedSearch { get; private set; }
        public IReadOnlyList<WorkspaceContextSearchHit> HitsToReturn { get; set; } = [];

        public Task<IReadOnlyList<WorkspaceContextSearchHit>> SearchAsync(
            WorkspaceContextSearch search,
            CancellationToken cancellationToken = default)
        {
            ReceivedSearch = search;
            return Task.FromResult(HitsToReturn);
        }
    }
}