using Espada.Domain.Rules;

namespace Espada.Domain.Errors
{
    public static class SyncConflictErrors
    {
        public static DomainError EntityTypeEmpty { get; } = new("SyncConflict.EntityTypeEmpty",
            "Sync conflict entity type cannot be empty.");

        public static DomainError DetailsEmpty { get; } = new("SyncConflict.DetailsEmpty",
            "Sync conflict details cannot be empty.");

        public static DomainError AlreadyResolved { get; } = new("SyncConflict.AlreadyResolved",
            "Sync conflict has already been resolved.");
    }
}