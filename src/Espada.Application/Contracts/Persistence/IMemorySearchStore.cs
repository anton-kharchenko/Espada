using Espada.Application.Models;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IMemorySearchStore
    {
        Task<IReadOnlyList<MemorySearchRecord>> LoadAsync(
            WorkspaceId workspaceId,
            IReadOnlyList<WorkspaceContextSearchHit> hits,
            IReadOnlyCollection<MemoryCategoryType> categoryTypes,
            CancellationToken cancellationToken = default);
    }
}