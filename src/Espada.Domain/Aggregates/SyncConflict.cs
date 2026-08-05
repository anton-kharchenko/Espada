using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class SyncConflict : AggregateRoot<SyncConflictId>, IHasConcurrencyVersion
    {
        private SyncConflict()
        {
        }

        private SyncConflict(SyncConflictId id, WorkspaceId workspaceId, string entityType, Guid entityId,
            SyncEventId localEventId, SyncEventId remoteEventId, string detailsJson, DateTimeOffset createdAtUtc)
            : base(id)
        {
            WorkspaceId = workspaceId;
            EntityType = entityType;
            EntityId = entityId;
            LocalEventId = localEventId;
            RemoteEventId = remoteEventId;
            DetailsJson = detailsJson;
            Status = SyncConflictStatusType.Open;
            CreatedAtUtc = createdAtUtc;
        }

        public WorkspaceId WorkspaceId { get; private set; } = null!;
        public string EntityType { get; private set; } = string.Empty;
        public Guid EntityId { get; private set; }
        public SyncEventId LocalEventId { get; private set; } = null!;
        public SyncEventId RemoteEventId { get; private set; } = null!;
        public string DetailsJson { get; private set; } = "{}";
        public SyncConflictStatusType Status { get; private set; } = null!;
        public DateTimeOffset CreatedAtUtc { get; private set; }
        public DateTimeOffset? ResolvedAtUtc { get; private set; }
        public uint Version { get; private set; }

        public static DomainResult<SyncConflict> Create(SyncConflictId id, WorkspaceId workspaceId,
            string? entityType, Guid entityId, SyncEventId localEventId, SyncEventId remoteEventId,
            string? detailsJson, DateTimeOffset createdAtUtc)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(workspaceId);
            ArgumentNullException.ThrowIfNull(localEventId);
            ArgumentNullException.ThrowIfNull(remoteEventId);
            if (string.IsNullOrWhiteSpace(entityType))
            {
                return DomainResult<SyncConflict>.Failure(SyncConflictErrors.EntityTypeEmpty);
            }

            return string.IsNullOrWhiteSpace(detailsJson)
                ? DomainResult<SyncConflict>.Failure(SyncConflictErrors.DetailsEmpty)
                : DomainResult<SyncConflict>.Success(new SyncConflict(id, workspaceId, entityType.Trim(), entityId,
                    localEventId, remoteEventId, detailsJson, createdAtUtc));
        }

        public DomainResult Resolve(DateTimeOffset resolvedAtUtc)
        {
            if (Status.Equals(SyncConflictStatusType.Resolved))
            {
                return DomainResult.Failure(SyncConflictErrors.AlreadyResolved);
            }

            Status = SyncConflictStatusType.Resolved;
            ResolvedAtUtc = resolvedAtUtc;
            return DomainResult.Success();
        }
    }
}