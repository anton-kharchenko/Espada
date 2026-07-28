using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;
using Espada.Domain.Enums;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class MemorySearchStoreSpy : IMemorySearchStore
    {
        public IReadOnlyList<MemorySearchRecord> RecordsToReturn { get; set; } = [];

        public Task<IReadOnlyList<MemorySearchRecord>> LoadAsync(
            WorkspaceId workspaceId,
            IReadOnlyList<WorkspaceContextSearchHit> hits,
            IReadOnlyCollection<MemoryCategoryType> categoryTypes,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RecordsToReturn);
        }
    }
}