using Espada.Domain.Rules;

namespace Espada.Domain.Errors
{
    public static class MemoryErrors
    {
        public static DomainError ConfidenceOutOfRange { get; } = new("Memory.ConfidenceOutOfRange",
            "Memory confidence must be between 0 and 1.");

        public static DomainError ClientIdentityEmpty { get; } =
            new("Memory.ClientIdentityEmpty", "Memory client identity cannot be empty.");

        public static DomainError IdentityTooLong { get; } = new("Memory.IdentityTooLong",
            "Memory client and session identities cannot exceed 200 characters.");

        public static DomainError RevisionMismatch { get; } =
            new("Memory.RevisionMismatch", "Memory revision must belong to the artifact.");

        public static DomainError MemoryKindRequired { get; } =
            new("Memory.MemoryKindRequired", "Memory metadata requires a memory artifact.");

        public static DomainError SupersedesSelf { get; } =
            new("Memory.SupersedesSelf", "Memory cannot supersede itself.");
    }
}