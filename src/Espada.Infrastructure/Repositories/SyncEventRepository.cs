using Espada.Application.Contracts.Persistence;
using Espada.Application.Models.Sync;
using Espada.Domain.Aggregates;
using Espada.Domain.Entities;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Espada.Infrastructure.Sync;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class SyncEventRepository(EspadaDbContext dbContext) : ISyncEventRepository
    {
        // ponytail: one local daemon owns writes; replace with a database allocator if multi-process local writes arrive.
        private static readonly SemaphoreSlim MaterializationLock = new(1, 1);

        public async Task MaterializeLocalStateAsync(DeviceId deviceId, bool includeSessionTranscripts,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(deviceId);
            await MaterializationLock.WaitAsync(cancellationToken);
            try
            {
                IReadOnlyList<SemanticSyncSnapshot> snapshots =
                    await SemanticSyncSnapshotFactory.CreateAsync(dbContext, includeSessionTranscripts,
                        cancellationToken);
                HashSet<(string EntityType, Guid EntityId, string PayloadHash)> existing =
                    (await dbContext.SyncEvents.AsNoTracking()
                        .Select(syncEvent => new
                        {
                            syncEvent.EntityType,
                            syncEvent.EntityId,
                            syncEvent.PayloadHash
                        })
                        .ToArrayAsync(cancellationToken))
                    .Select(item => (item.EntityType, item.EntityId, item.PayloadHash))
                    .ToHashSet();
                long sequence = await dbContext.SyncEvents.AsNoTracking()
                    .Where(syncEvent => syncEvent.DeviceId == deviceId)
                    .MaxAsync(syncEvent => (long?)syncEvent.Sequence, cancellationToken) ?? 0;
                foreach (SemanticSyncSnapshot snapshot in snapshots
                             .OrderBy(item => GetEntityOrder(item.EntityType))
                             .ThenBy(item => item.Timestamp)
                             .ThenBy(item => item.EntityId))
                {
                    if (!existing.Add((snapshot.EntityType, snapshot.EntityId, snapshot.PayloadHash)))
                    {
                        continue;
                    }

                    SyncEvent syncEvent = SyncEvent.Create(CreateEventId(deviceId, snapshot), deviceId, ++sequence,
                        WorkspaceId.Create(snapshot.WorkspaceId), snapshot.EntityType, snapshot.EntityId,
                        snapshot.Operation, snapshot.BaseVersion, snapshot.Timestamp, snapshot.PayloadType,
                        snapshot.PayloadJson, snapshot.PayloadHash).Value;
                    await dbContext.SyncEvents.AddAsync(syncEvent, cancellationToken);
                }

                await dbContext.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                MaterializationLock.Release();
            }
        }

        public async Task<IReadOnlyList<SyncEvent>> ListPendingAsync(DeviceId deviceId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(deviceId);
            Dictionary<Guid, long> pushed = await dbContext.SyncCursors.AsNoTracking()
                .Where(cursor => cursor.DeviceId == deviceId)
                .ToDictionaryAsync(cursor => cursor.WorkspaceId.Value, cursor => cursor.LastPushedSequence,
                    cancellationToken);
            SyncEvent[] events = await dbContext.SyncEvents.AsNoTracking()
                .Where(syncEvent => syncEvent.DeviceId == deviceId)
                .OrderBy(syncEvent => syncEvent.Sequence)
                .ToArrayAsync(cancellationToken);
            return events.Where(syncEvent =>
                    syncEvent.Sequence > pushed.GetValueOrDefault(syncEvent.WorkspaceId.Value))
                .ToArray();
        }

        public async Task<SyncCursor> GetOrCreateCursorAsync(DeviceId deviceId, WorkspaceId workspaceId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(deviceId);
            ArgumentNullException.ThrowIfNull(workspaceId);
            SyncCursor? cursor = await dbContext.SyncCursors.SingleOrDefaultAsync(
                candidate => candidate.DeviceId == deviceId && candidate.WorkspaceId == workspaceId,
                cancellationToken);
            if (cursor is not null)
            {
                return cursor;
            }

            cursor = SyncCursor.Create(SyncCursorId.New(), deviceId, workspaceId, "0",
                DateTimeOffset.UtcNow).Value;
            await dbContext.SyncCursors.AddAsync(cursor, cancellationToken);
            return cursor;
        }

        public async Task<IReadOnlyList<StoredSyncEvent>> ListAfterServerSequenceAsync(
            IReadOnlyCollection<WorkspaceId> workspaceIds, long serverSequence, int limit,
            CancellationToken cancellationToken = default)
        {
            WorkspaceId[] ids = workspaceIds.ToArray();
            return await dbContext.SyncEvents.AsNoTracking()
                .Where(syncEvent => ids.Contains(syncEvent.WorkspaceId)
                                    && EF.Property<long>(syncEvent, "ServerSequence") > serverSequence)
                .OrderBy(syncEvent => EF.Property<long>(syncEvent, "ServerSequence"))
                .Take(limit)
                .Select(syncEvent => new StoredSyncEvent(syncEvent,
                    EF.Property<long>(syncEvent, "ServerSequence")))
                .ToArrayAsync(cancellationToken);
        }

        public Task<SyncEvent?> GetByIdAsync(SyncEventId eventId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(eventId);
            return dbContext.SyncEvents.AsNoTracking()
                .SingleOrDefaultAsync(syncEvent => syncEvent.Id == eventId, cancellationToken);
        }

        public Task<SyncEvent?> GetLatestEntityEventAsync(WorkspaceId workspaceId, string entityType, Guid entityId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);
            return dbContext.SyncEvents.AsNoTracking()
                .Where(syncEvent => syncEvent.WorkspaceId == workspaceId
                                    && syncEvent.EntityType == entityType
                                    && syncEvent.EntityId == entityId)
                .OrderByDescending(syncEvent => EF.Property<long>(syncEvent, "ServerSequence"))
                .FirstOrDefaultAsync(cancellationToken);
        }
        public Task<SyncEvent?> GetByDeviceSequenceAsync(DeviceId deviceId, long sequence,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(deviceId);
            return dbContext.SyncEvents.AsNoTracking()
                .SingleOrDefaultAsync(syncEvent => syncEvent.DeviceId == deviceId
                                                   && syncEvent.Sequence == sequence, cancellationToken);
        }

        public async Task AddAsync(SyncEvent syncEvent, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(syncEvent);
            await dbContext.SyncEvents.AddAsync(syncEvent, cancellationToken);
        }

        public async Task<long> GetServerSequenceAsync(SyncEventId eventId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(eventId);
            return await dbContext.SyncEvents.AsNoTracking()
                .Where(syncEvent => syncEvent.Id == eventId)
                .Select(syncEvent => EF.Property<long>(syncEvent, "ServerSequence"))
                .SingleAsync(cancellationToken);
        }

        private static int GetEntityOrder(string entityType)
        {
            return entityType switch
            {
                nameof(Workspace) => 0,
                nameof(Project) => 10,
                nameof(ProjectTask) => 20,
                nameof(Source) => 30,
                nameof(Artifact) => 40,
                nameof(ArtifactRevision) => 50,
                nameof(Binding) => 60,
                nameof(AgentProfile) => 70,
                nameof(AgentSession) => 80,
                nameof(AgentSessionEvent) => 90,
                nameof(ChunkBatch) => 100,
                nameof(ImportJob) => 110,
                nameof(Chunk) => 120,
                nameof(ChunkEmbedding) => 130,
                _ => 1000
            };
        }

        private static SyncEventId CreateEventId(DeviceId deviceId, SemanticSyncSnapshot snapshot)
        {
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(
                $"{deviceId.Value:D}:{snapshot.EntityType}:{snapshot.EntityId:D}:{snapshot.PayloadHash}"));
            hash[7] = (byte)((hash[7] & 0x0F) | 0x50);
            hash[8] = (byte)((hash[8] & 0x3F) | 0x80);
            return SyncEventId.Create(new Guid(hash.AsSpan(0, 16)));
        }
    }
}