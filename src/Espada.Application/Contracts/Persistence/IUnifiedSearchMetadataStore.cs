using Espada.Application.Models;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IUnifiedSearchMetadataStore
    {
        Task<IReadOnlyList<UnifiedSearchRecord>> LoadAsync(WorkspaceId workspaceId,
            IReadOnlyList<WorkspaceContextSearchHit> hits, CancellationToken cancellationToken = default);
    }
}