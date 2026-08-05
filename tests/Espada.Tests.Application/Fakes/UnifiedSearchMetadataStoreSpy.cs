using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class UnifiedSearchMetadataStoreSpy : IUnifiedSearchMetadataStore
    {
        public IReadOnlyList<WorkspaceContextSearchHit>? ReceivedHits { get; private set; }
        public IReadOnlyList<UnifiedSearchRecord> RecordsToReturn { get; set; } = [];

        public Task<IReadOnlyList<UnifiedSearchRecord>> LoadAsync(WorkspaceId workspaceId,
            IReadOnlyList<WorkspaceContextSearchHit> hits, CancellationToken cancellationToken = default)
        {
            ReceivedHits = hits;
            return Task.FromResult(RecordsToReturn);
        }
    }
}