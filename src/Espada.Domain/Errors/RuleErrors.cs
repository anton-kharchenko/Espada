using Espada.Domain.Rules;

namespace Espada.Domain.Errors
{
    public static class RuleErrors
    {
        public static DomainError KeyEmpty { get; } = new("Rule.KeyEmpty", "Rule key cannot be empty.");

        public static DomainError KeyTooLong { get; } =
            new("Rule.KeyTooLong", "Rule key cannot exceed 100 characters.");

        public static DomainError KeyInvalid { get; } = new("Rule.KeyInvalid",
            "Rule key can contain only letters, numbers, periods, underscores and hyphens.");

        public static DomainError TextEmpty { get; } = new("Rule.TextEmpty", "Rule text cannot be empty.");

        public static DomainError RevisionMismatch { get; } =
            new("Rule.RevisionMismatch", "Rule revision must belong to the artifact.");

        public static DomainError InstructionKindRequired { get; } = new("Rule.InstructionKindRequired",
            "Instruction rules require an instruction artifact.");

        public static DomainError PolicyKindRequired { get; } =
            new("Rule.PolicyKindRequired", "Policy rules require a policy artifact.");
    }
}