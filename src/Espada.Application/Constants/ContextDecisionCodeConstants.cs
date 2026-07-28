namespace Espada.Application.Constants
{
    public static class ContextDecisionCodeConstants
    {
        public const string Included = "included";
        public const string SelectorMismatch = "selector_mismatch";
        public const string ArchivedArtifact = "archived_artifact";
        public const string RedundantBinding = "redundant_binding";
        public const string InvalidTypedGraph = "invalid_typed_graph";
        public const string BlockedByHardPolicy = "blocked_by_hard_policy";
        public const string OverriddenSoftRule = "overridden_soft_rule";
        public const string DuplicateRule = "duplicate_rule";
        public const string SupersededMemory = "superseded_memory";
        public const string BudgetExceeded = "budget_exceeded";
        public const string ArtifactKindNotContextual = "artifact_kind_not_contextual";

        public const string HardPolicyConflict = "hard_policy_conflict";
        public const string AmbiguousSoftRule = "ambiguous_soft_rule";
    }
}