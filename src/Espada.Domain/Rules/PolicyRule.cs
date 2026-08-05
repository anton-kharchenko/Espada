using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Rules
{
    public sealed class PolicyRule
    {
        private PolicyRule()
        {
        }

        private PolicyRule(ArtifactRevisionId artifactRevisionId, ArtifactKindType kindType, RuleKey ruleKey,
            string text, ContextPriority priority, PolicyEnforcementType enforcementType)
        {
            ArtifactRevisionId = artifactRevisionId;
            KindType = kindType;
            RuleKey = ruleKey;
            Text = text;
            Priority = priority;
            EnforcementType = enforcementType;
        }

        public ArtifactRevisionId ArtifactRevisionId { get; private set; } = null!;
        public ArtifactKindType KindType { get; private set; } = null!;
        public RuleKey RuleKey { get; private set; } = null!;
        public string Text { get; private set; } = string.Empty;
        public ContextPriority Priority { get; private set; } = ContextPriority.Neutral;
        public PolicyEnforcementType EnforcementType { get; private set; } = null!;

        internal static DomainResult<PolicyRule> Create(ArtifactRevision revision, RuleKey ruleKey, string? text,
            ContextPriority priority, PolicyEnforcementType enforcementType)
        {
            ArgumentNullException.ThrowIfNull(revision);
            ArgumentNullException.ThrowIfNull(ruleKey);
            ArgumentNullException.ThrowIfNull(priority);
            ArgumentNullException.ThrowIfNull(enforcementType);
            return string.IsNullOrWhiteSpace(text)
                ? DomainResult<PolicyRule>.Failure(RuleErrors.TextEmpty)
                : DomainResult<PolicyRule>.Success(new PolicyRule(revision.Id, ArtifactKindType.Policy, ruleKey,
                    text.Trim(), priority, enforcementType));
        }
    }
}