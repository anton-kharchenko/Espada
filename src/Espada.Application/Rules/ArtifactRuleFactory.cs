using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;
using Espada.Application.UseCases.Artifacts.Common;

namespace Espada.Application.Rules
{
    internal static class ArtifactRuleFactory
    {
        private static readonly DomainError RulesRequired = new(
            "Artifact.Rules.Required",
            "Instruction and policy artifacts require at least one typed rule.");

        private static readonly DomainError RulesNotAllowed = new(
            "Artifact.Rules.NotAllowed",
            "Document artifacts cannot contain typed rules.");

        private static DomainError DuplicateRuleKey(string ruleKey)
        {
            return new DomainError(
                "Artifact.RuleKey.Duplicate",
                $"Rule key '{ruleKey}' must be unique within an artifact revision.");
        }

        public static DomainResult<ArtifactRuleSet> Create(
            Artifact artifact,
            ArtifactRevision revision,
            IReadOnlyList<InstructionRuleInput>? instructionInputs,
            IReadOnlyList<PolicyRuleInput>? policyInputs)
        {
            instructionInputs ??= [];
            policyInputs ??= [];

            if (artifact.KindType.Equals(ArtifactKindType.Document))
            {
                return instructionInputs.Count == 0 && policyInputs.Count == 0
                    ? DomainResult.Success(new ArtifactRuleSet([], []))
                    : DomainResult.Failure<ArtifactRuleSet>(RulesNotAllowed);
            }

            if (artifact.KindType.Equals(ArtifactKindType.Instruction))
            {
                if (instructionInputs.Count == 0 || policyInputs.Count != 0)
                {
                    return DomainResult.Failure<ArtifactRuleSet>(RulesRequired);
                }

                List<InstructionRule> rules = [];
                HashSet<string> ruleKeys = new(StringComparer.Ordinal);
                foreach (InstructionRuleInput input in instructionInputs)
                {
                    DomainResult<RuleKey> keyResult = RuleKey.Create(input.RuleKey);
                    DomainResult<ContextPriority> priorityResult = ContextPriority.Create(input.Priority);
                    if (keyResult.IsFailure || priorityResult.IsFailure)
                    {
                        return DomainResult.Failure<ArtifactRuleSet>(
                            keyResult.IsFailure ? keyResult.Error : priorityResult.Error);
                    }

                    if (!ruleKeys.Add(keyResult.Value.Value))
                    {
                        return DomainResult.Failure<ArtifactRuleSet>(
                            DuplicateRuleKey(keyResult.Value.Value));
                    }

                    DomainResult<InstructionRule> ruleResult = artifact.CreateInstructionRule(
                        revision,
                        keyResult.Value,
                        input.Text,
                        priorityResult.Value);
                    if (ruleResult.IsFailure)
                    {
                        return DomainResult.Failure<ArtifactRuleSet>(ruleResult.Error);
                    }

                    rules.Add(ruleResult.Value);
                }

                return DomainResult.Success(new ArtifactRuleSet(rules, []));
            }

            if (artifact.KindType.Equals(ArtifactKindType.Policy))
            {
                if (policyInputs.Count == 0 || instructionInputs.Count != 0)
                {
                    return DomainResult.Failure<ArtifactRuleSet>(RulesRequired);
                }

                List<PolicyRule> rules = [];
                HashSet<string> ruleKeys = new(StringComparer.Ordinal);
                foreach (PolicyRuleInput input in policyInputs)
                {
                    DomainResult<RuleKey> keyResult = RuleKey.Create(input.RuleKey);
                    DomainResult<ContextPriority> priorityResult = ContextPriority.Create(input.Priority);
                    PolicyEnforcementType? enforcementType = Enumeration
                        .GetAll<PolicyEnforcementType>()
                        .SingleOrDefault(value => value.Id == input.EnforcementTypeId);
                    if (keyResult.IsFailure || priorityResult.IsFailure || enforcementType is null)
                    {
                        DomainError error = keyResult.IsFailure
                            ? keyResult.Error
                            : priorityResult.IsFailure
                                ? priorityResult.Error
                                : new DomainError(
                                    "Policy.EnforcementType.Unsupported",
                                    $"Policy enforcement type with ID '{input.EnforcementTypeId}' is not supported.");
                        return DomainResult.Failure<ArtifactRuleSet>(error);
                    }

                    if (!ruleKeys.Add(keyResult.Value.Value))
                    {
                        return DomainResult.Failure<ArtifactRuleSet>(
                            DuplicateRuleKey(keyResult.Value.Value));
                    }

                    DomainResult<PolicyRule> ruleResult = artifact.CreatePolicyRule(
                        revision,
                        keyResult.Value,
                        input.Text,
                        priorityResult.Value,
                        enforcementType);
                    if (ruleResult.IsFailure)
                    {
                        return DomainResult.Failure<ArtifactRuleSet>(ruleResult.Error);
                    }

                    rules.Add(ruleResult.Value);
                }

                return DomainResult.Success(new ArtifactRuleSet([], rules));
            }

            return DomainResult.Failure<ArtifactRuleSet>(
                new DomainError(
                    "Artifact.KindType.Unsupported",
                    $"Artifact kind '{artifact.KindType.Name}' is not supported by artifact.create."));
        }
    }
}