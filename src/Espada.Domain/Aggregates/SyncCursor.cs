using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class SyncCursor : AggregateRoot<SyncCursorId>, IHasConcurrencyVersion
    {
        private SyncCursor()
        {
        }

        private SyncCursor(SyncCursorId id, DeviceId deviceId, WorkspaceId workspaceId, string serverCursor,
            DateTimeOffset updatedAtUtc) : base(id)
        {
            DeviceId = deviceId;
            WorkspaceId = workspaceId;
            ServerCursor = serverCursor;
            UpdatedAtUtc = updatedAtUtc;
        }

        public DeviceId DeviceId { get; private set; } = null!;
        public WorkspaceId WorkspaceId { get; private set; } = null!;
        public string ServerCursor { get; private set; } = string.Empty;
        public long LastPushedSequence { get; private set; }
        public DateTimeOffset UpdatedAtUtc { get; private set; }
        public uint Version { get; private set; }

        public static DomainResult<SyncCursor> Create(SyncCursorId id, DeviceId deviceId, WorkspaceId workspaceId,
            string? serverCursor, DateTimeOffset updatedAtUtc)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(deviceId);
            ArgumentNullException.ThrowIfNull(workspaceId);
            return string.IsNullOrWhiteSpace(serverCursor)
                ? DomainResult<SyncCursor>.Failure(SyncCursorErrors.ServerCursorEmpty)
                : DomainResult<SyncCursor>.Success(new SyncCursor(id, deviceId, workspaceId, serverCursor.Trim(),
                    updatedAtUtc));
        }

        public DomainResult AdvancePush(long sequence, DateTimeOffset updatedAtUtc)
        {
            if (sequence < LastPushedSequence)
            {
                return DomainResult.Failure(SyncCursorErrors.PushSequenceCannotMoveBackward);
            }

            LastPushedSequence = sequence;
            UpdatedAtUtc = updatedAtUtc;
            return DomainResult.Success();
        }

        public DomainResult Advance(string? serverCursor, DateTimeOffset updatedAtUtc)
        {
            if (string.IsNullOrWhiteSpace(serverCursor))
            {
                return DomainResult.Failure(SyncCursorErrors.ServerCursorEmpty);
            }

            ServerCursor = serverCursor.Trim();
            UpdatedAtUtc = updatedAtUtc;
            return DomainResult.Success();
        }
    }
}