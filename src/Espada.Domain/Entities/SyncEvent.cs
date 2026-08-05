using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Entities
{
    public sealed class SyncEvent : Entity<SyncEventId>
    {
        private SyncEvent()
        {
        }

        private SyncEvent(SyncEventId id, DeviceId deviceId, long sequence, WorkspaceId workspaceId,
            string entityType, Guid entityId, string operation, uint? baseVersion, DateTimeOffset occurredAtUtc,
            string payloadType, string payloadJson, string payloadHash) : base(id)
        {
            DeviceId = deviceId;
            Sequence = sequence;
            WorkspaceId = workspaceId;
            EntityType = entityType;
            EntityId = entityId;
            Operation = operation;
            BaseVersion = baseVersion;
            OccurredAtUtc = occurredAtUtc;
            PayloadType = payloadType;
            PayloadJson = payloadJson;
            PayloadHash = payloadHash;
        }

        public DeviceId DeviceId { get; private set; } = null!;
        public long Sequence { get; private set; }
        public WorkspaceId WorkspaceId { get; private set; } = null!;
        public string EntityType { get; private set; } = string.Empty;
        public Guid EntityId { get; private set; }
        public string Operation { get; private set; } = string.Empty;
        public uint? BaseVersion { get; private set; }
        public DateTimeOffset OccurredAtUtc { get; private set; }
        public string PayloadType { get; private set; } = string.Empty;
        public string PayloadJson { get; private set; } = "{}";
        public string PayloadHash { get; private set; } = string.Empty;

        public static DomainResult<SyncEvent> Create(SyncEventId id, DeviceId deviceId, long sequence,
            WorkspaceId workspaceId, string? entityType, Guid entityId, string? operation, uint? baseVersion,
            DateTimeOffset occurredAtUtc, string? payloadType, string? payloadJson, string? payloadHash)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(deviceId);
            ArgumentNullException.ThrowIfNull(workspaceId);
            if (sequence < 1)
            {
                return DomainResult<SyncEvent>.Failure(SyncEventErrors.SequenceOutOfRange);
            }

            if (string.IsNullOrWhiteSpace(entityType))
            {
                return DomainResult<SyncEvent>.Failure(SyncEventErrors.EntityTypeEmpty);
            }

            if (string.IsNullOrWhiteSpace(operation))
            {
                return DomainResult<SyncEvent>.Failure(SyncEventErrors.OperationEmpty);
            }

            if (string.IsNullOrWhiteSpace(payloadType))
            {
                return DomainResult<SyncEvent>.Failure(SyncEventErrors.PayloadTypeEmpty);
            }

            if (string.IsNullOrWhiteSpace(payloadJson))
            {
                return DomainResult<SyncEvent>.Failure(SyncEventErrors.PayloadEmpty);
            }

            return string.IsNullOrWhiteSpace(payloadHash)
                ? DomainResult<SyncEvent>.Failure(SyncEventErrors.PayloadHashEmpty)
                : DomainResult<SyncEvent>.Success(new SyncEvent(id, deviceId, sequence, workspaceId,
                    entityType.Trim(), entityId, operation.Trim(), baseVersion, occurredAtUtc, payloadType.Trim(),
                    payloadJson, payloadHash.Trim()));
        }
    }
}