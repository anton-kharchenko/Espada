using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Rules
{
    public sealed class InstructionRule
    {
        private InstructionRule()
        {
        }

        private InstructionRule(ArtifactRevisionId artifactRevisionId, ArtifactKindType kindType, RuleKey ruleKey,
            string text, ContextPriority priority)
        {
            ArtifactRevisionId = artifactRevisionId;
            KindType = kindType;
            RuleKey = ruleKey;
            Text = text;
            Priority = priority;
        }

        public ArtifactRevisionId ArtifactRevisionId { get; private set; } = null!;
        public ArtifactKindType KindType { get; private set; } = null!;
        public RuleKey RuleKey { get; private set; } = null!;
        public string Text { get; private set; } = string.Empty;
        public ContextPriority Priority { get; private set; } = ContextPriority.Neutral;

        internal static DomainResult<InstructionRule> Create(ArtifactRevision revision, RuleKey ruleKey, string? text,
            ContextPriority priority)
        {
            ArgumentNullException.ThrowIfNull(revision);
            ArgumentNullException.ThrowIfNull(ruleKey);
            ArgumentNullException.ThrowIfNull(priority);
            return string.IsNullOrWhiteSpace(text)
                ? DomainResult<InstructionRule>.Failure(RuleErrors.TextEmpty)
                : DomainResult<InstructionRule>.Success(new InstructionRule(revision.Id, ArtifactKindType.Instruction,
                    ruleKey, text.Trim(), priority));
        }
    }
}