using Espada.Domain.Rules;

namespace Espada.Application.Rules
{
    internal sealed record ArtifactRuleSet(
        IReadOnlyList<InstructionRule> InstructionRules,
        IReadOnlyList<PolicyRule> PolicyRules);
}