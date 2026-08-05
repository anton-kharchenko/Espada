using Espada.Domain.Rules;

namespace Espada.Domain.Errors
{
    public static class SyncEventErrors
    {
        public static DomainError SequenceOutOfRange { get; } = new("SyncEvent.SequenceOutOfRange",
            "Sync event sequence must be positive.");

        public static DomainError EntityTypeEmpty { get; } = new("SyncEvent.EntityTypeEmpty",
            "Sync event entity type cannot be empty.");

        public static DomainError OperationEmpty { get; } = new("SyncEvent.OperationEmpty",
            "Sync event operation cannot be empty.");

        public static DomainError PayloadTypeEmpty { get; } = new("SyncEvent.PayloadTypeEmpty",
            "Sync event payload type cannot be empty.");

        public static DomainError PayloadEmpty { get; } = new("SyncEvent.PayloadEmpty",
            "Sync event payload cannot be empty.");

        public static DomainError PayloadHashEmpty { get; } = new("SyncEvent.PayloadHashEmpty",
            "Sync event payload hash cannot be empty.");
    }
}