using Espada.Application.Models.Sync;
using Espada.Domain.Aggregates;
using Espada.Domain.Entities;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface ISyncEventRepository
    {
        Task MaterializeLocalStateAsync(DeviceId deviceId, bool includeSessionTranscripts,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SyncEvent>> ListPendingAsync(DeviceId deviceId,
            CancellationToken cancellationToken = default);
        Task<SyncCursor> GetOrCreateCursorAsync(DeviceId deviceId, WorkspaceId workspaceId,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StoredSyncEvent>> ListAfterServerSequenceAsync(IReadOnlyCollection<WorkspaceId> workspaceIds,
            long serverSequence, int limit, CancellationToken cancellationToken = default);
        Task<SyncEvent?> GetByIdAsync(SyncEventId eventId, CancellationToken cancellationToken = default);
        Task<SyncEvent?> GetLatestEntityEventAsync(WorkspaceId workspaceId, string entityType, Guid entityId,
            CancellationToken cancellationToken = default);
        Task<SyncEvent?> GetByDeviceSequenceAsync(DeviceId deviceId, long sequence,
            CancellationToken cancellationToken = default);
        Task AddAsync(SyncEvent syncEvent, CancellationToken cancellationToken = default);
        Task<long> GetServerSequenceAsync(SyncEventId eventId, CancellationToken cancellationToken = default);
    }
}