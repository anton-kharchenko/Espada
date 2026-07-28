using Espada.Application.Models;

namespace Espada.Application.Contracts.Persistence
{
    public interface IWorkspaceContextSearchStore
    {
        Task<IReadOnlyList<WorkspaceContextSearchHit>> SearchAsync(
            WorkspaceContextSearch search,
            CancellationToken cancellationToken = default);
    }
}