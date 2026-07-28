using Espada.Application.ApplicationErrors;
using Espada.Application.Constants;
using Espada.Application.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using System.Text;

namespace Espada.Application.Services
{
    public sealed class ContextResolver
    {
        public DomainResult<ResolvedContext> Resolve(
            ContextResolutionRequest request,
            IReadOnlyList<ContextCandidateRecord> candidates)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(candidates);

            List<ResolvedContextItem> eligible = [];
            List<ResolvedContextItem> excluded = [];
            List<ContextConflict> conflicts = [];

            foreach (ContextCandidateRecord candidate in candidates
                         .OrderBy(value => value.Binding.Id.Value))
            {
                IReadOnlyList<ContextSelectorMatch> selectors = MatchSelectors(
                    request,
                    candidate.Binding);
                ContextSpecificity specificity = CalculateSpecificity(candidate.Binding);
                IReadOnlyList<ResolvedContextItem> items = ExpandCandidate(
                    candidate,
                    specificity,
                    selectors);
                bool selectorsMatched = selectors.All(selector => selector.Matched);

                foreach (ResolvedContextItem item in items)
                {
                    if (!selectorsMatched)
                    {
                        string mismatches = string.Join(
                            ", ",
                            selectors
                                .Where(selector => !selector.Matched)
                                .Select(selector => selector.Selector));
                        excluded.Add(WithDecision(
                            item,
                            ContextDecisionCodeConstants.SelectorMismatch,
                            $"Binding selectors did not match: {mismatches}."));
                        continue;
                    }

                    if (candidate.Artifact.Status.Equals(ArtifactStatusType.Archived))
                    {
                        excluded.Add(WithDecision(
                            item,
                            ContextDecisionCodeConstants.ArchivedArtifact,
                            "The bound artifact is archived."));
                        continue;
                    }

                    if (item.DecisionCode.Equals(
                            ContextDecisionCodeConstants.InvalidTypedGraph,
                            StringComparison.Ordinal))
                    {
                        excluded.Add(item);
                        conflicts.Add(new ContextConflict(
                            $"artifact:{candidate.Artifact.Id.Value:D}",
                            ContextDecisionCodeConstants.InvalidTypedGraph,
                            [candidate.Artifact.Id.Value],
                            null,
                            "The artifact revision is missing its required typed payload."));
                        continue;
                    }

                    if (item.DecisionCode.Equals(
                            ContextDecisionCodeConstants.ArtifactKindNotContextual,
                            StringComparison.Ordinal))
                    {
                        excluded.Add(item);
                        continue;
                    }

                    if (candidate.IsMemorySuperseded)
                    {
                        excluded.Add(WithDecision(
                            item,
                            ContextDecisionCodeConstants.SupersededMemory,
                            "A newer memory supersedes this memory."));
                        continue;
                    }

                    eligible.Add(item);
                }
            }

            eligible = DeduplicateBindings(eligible, excluded);

            List<ResolvedContextItem> hardPolicies = eligible
                .Where(IsHardPolicy)
                .ToList();
            List<ResolvedContextItem> softRules = eligible
                .Where(item => item.RuleKey is not null && !IsHardPolicy(item))
                .ToList();
            List<ResolvedContextItem> memories = eligible
                .Where(item => item.MemoryMetadata is not null)
                .ToList();

            List<ResolvedContextItem> selectedHard = [];
            HashSet<string> hardRuleKeys = new(StringComparer.Ordinal);

            foreach (IGrouping<string, ResolvedContextItem> group in hardPolicies
                         .GroupBy(item => item.RuleKey!, StringComparer.Ordinal)
                         .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                hardRuleKeys.Add(group.Key);
                List<ResolvedContextItem> ranked = OrderRules(group);
                HashSet<string> texts = new(StringComparer.Ordinal);
                List<ResolvedContextItem> distinct = [];

                foreach (ResolvedContextItem item in ranked)
                {
                    if (texts.Add(item.Content))
                    {
                        distinct.Add(item);
                    }
                    else
                    {
                        excluded.Add(WithDecision(
                            item,
                            ContextDecisionCodeConstants.DuplicateRule,
                            $"An identical hard policy with rule key '{group.Key}' was already selected."));
                    }
                }

                selectedHard.AddRange(distinct);
                if (distinct.Count > 1)
                {
                    conflicts.Add(new ContextConflict(
                        group.Key,
                        ContextDecisionCodeConstants.HardPolicyConflict,
                        distinct
                            .Select(item => item.Artifact.Id.Value)
                            .Distinct()
                            .Order()
                            .ToArray(),
                        null,
                        "Different hard policy texts share the same rule key; all are retained."));
                }
            }

            List<ResolvedContextItem> selectedSoft = [];
            foreach (ResolvedContextItem blocked in softRules
                         .Where(item => hardRuleKeys.Contains(item.RuleKey!)))
            {
                excluded.Add(WithDecision(
                    blocked,
                    ContextDecisionCodeConstants.BlockedByHardPolicy,
                    $"Hard policy '{blocked.RuleKey}' prevents a soft replacement."));
            }

            foreach (IGrouping<string, ResolvedContextItem> group in softRules
                         .Where(item => !hardRuleKeys.Contains(item.RuleKey!))
                         .GroupBy(item => item.RuleKey!, StringComparer.Ordinal)
                         .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                List<ResolvedContextItem> ranked = OrderRules(group);
                ResolvedContextItem winner = ranked[0];
                selectedSoft.Add(winner);

                ResolvedContextItem[] ambiguous = ranked
                    .Where(item => item.Specificity == winner.Specificity
                                   && !item.Content.Equals(
                                       winner.Content,
                                       StringComparison.Ordinal))
                    .ToArray();
                if (ambiguous.Length > 0)
                {
                    conflicts.Add(new ContextConflict(
                        group.Key,
                        ContextDecisionCodeConstants.AmbiguousSoftRule,
                        ambiguous
                            .Prepend(winner)
                            .Select(item => item.Artifact.Id.Value)
                            .Distinct()
                            .Order()
                            .ToArray(),
                        winner.Artifact.Id.Value,
                        "Different soft rule texts have equal specificity; deterministic priority and identity ordering selected the winner."));
                }

                foreach (ResolvedContextItem loser in ranked.Skip(1))
                {
                    bool duplicate = loser.Content.Equals(
                        winner.Content,
                        StringComparison.Ordinal);
                    excluded.Add(WithDecision(
                        loser,
                        duplicate
                            ? ContextDecisionCodeConstants.DuplicateRule
                            : ContextDecisionCodeConstants.OverriddenSoftRule,
                        duplicate
                            ? $"An identical soft rule with key '{group.Key}' was already selected."
                            : $"A more specific or higher-ranked soft rule with key '{group.Key}' was selected."));
                }
            }

            selectedHard = OrderRules(selectedHard);
            selectedSoft = OrderRules(selectedSoft);
            List<ResolvedContextItem> confirmedMemories = OrderMemories(
                memories.Where(item => item.MemoryMetadata!.UserConfirmed));
            List<ResolvedContextItem> unconfirmedMemories = OrderMemories(
                memories.Where(item => !item.MemoryMetadata!.UserConfirmed));

            long hardPolicyBytes = selectedHard.Sum(item => (long)item.SizeInBytes);
            if (hardPolicyBytes > request.TokenBudget)
            {
                return DomainResult.Failure<ResolvedContext>(
                    ContextApplicationErrors.BudgetTooSmall);
            }

            List<ResolvedContextItem> included = selectedHard
                .Select(item => WithDecision(
                    item,
                    ContextDecisionCodeConstants.Included,
                    "Hard policy matched and is mandatory."))
                .ToList();
            int usedBytes = (int)hardPolicyBytes;
            IEnumerable<ResolvedContextItem> optionalItems = selectedSoft
                .Concat(confirmedMemories)
                .Concat(unconfirmedMemories);

            foreach (ResolvedContextItem item in optionalItems)
            {
                if (item.SizeInBytes <= request.TokenBudget - usedBytes)
                {
                    included.Add(WithDecision(
                        item,
                        ContextDecisionCodeConstants.Included,
                        item.MemoryMetadata?.UserConfirmed == false
                            ? "Unconfirmed memory matched and fit the remaining budget."
                            : "The item matched and fit the remaining budget."));
                    usedBytes += item.SizeInBytes;
                }
                else
                {
                    excluded.Add(WithDecision(
                        item,
                        ContextDecisionCodeConstants.BudgetExceeded,
                        $"The complete {item.SizeInBytes}-byte item did not fit the remaining {request.TokenBudget - usedBytes} bytes."));
                }
            }

            excluded = OrderExcluded(excluded);
            conflicts = conflicts
                .OrderBy(conflict => conflict.RuleKey, StringComparer.Ordinal)
                .ThenBy(conflict => conflict.ConflictCode, StringComparer.Ordinal)
                .ToList();
            ContextExplanation[] explanations = included
                .Concat(excluded)
                .Select(item => new ContextExplanation(
                    item.Binding.Id.Value,
                    item.Artifact.Id.Value,
                    item.Revision.Id.Value,
                    item.DecisionCode,
                    item.Explanation))
                .ToArray();
            ContextBudgetSummary budget = new(
                request.TokenBudget,
                (int)hardPolicyBytes,
                usedBytes,
                request.TokenBudget - usedBytes,
                included.Count,
                excluded.Count);
            ResolvedContext resolvedContext = new(
                request.Workspace,
                request.Project,
                request.Task,
                request.Project?.CanonicalRemoteUri,
                request.RepositoryRelativePath,
                request.Branch,
                request.Agent,
                included,
                excluded,
                conflicts,
                explanations,
                budget);

            return DomainResult.Success(resolvedContext);
        }

        private static IReadOnlyList<ResolvedContextItem> ExpandCandidate(
            ContextCandidateRecord candidate,
            ContextSpecificity specificity,
            IReadOnlyList<ContextSelectorMatch> selectors)
        {
            if (candidate.Artifact.KindType.Equals(ArtifactKindType.Instruction))
            {
                if (candidate.InstructionRules.Count == 0
                    || candidate.InstructionRules.Any(rule => !rule.ArtifactRevisionId.Equals(candidate.Revision.Id)))
                {
                    return [CreateInvalidTypedItem(candidate, specificity, selectors)];
                }

                return candidate.InstructionRules
                    .OrderBy(rule => rule.RuleKey.Value, StringComparer.Ordinal)
                    .Select(rule => CreateItem(
                        candidate,
                        rule.RuleKey.Value,
                        null,
                        rule.Text,
                        rule.Priority.Value,
                        null,
                        specificity,
                        selectors,
                        ContextDecisionCodeConstants.Included,
                        "Instruction rule is eligible for resolution."))
                    .ToArray();
            }

            if (candidate.Artifact.KindType.Equals(ArtifactKindType.Policy))
            {
                if (candidate.PolicyRules.Count == 0
                    || candidate.PolicyRules.Any(rule => !rule.ArtifactRevisionId.Equals(candidate.Revision.Id)))
                {
                    return [CreateInvalidTypedItem(candidate, specificity, selectors)];
                }

                return candidate.PolicyRules
                    .OrderBy(rule => rule.RuleKey.Value, StringComparer.Ordinal)
                    .Select(rule => CreateItem(
                        candidate,
                        rule.RuleKey.Value,
                        rule.EnforcementType.Name.ToLowerInvariant(),
                        rule.Text,
                        rule.Priority.Value,
                        null,
                        specificity,
                        selectors,
                        ContextDecisionCodeConstants.Included,
                        "Policy rule is eligible for resolution."))
                    .ToArray();
            }

            if (candidate.Artifact.KindType.Equals(ArtifactKindType.Memory))
            {
                MemoryMetadata? metadata = candidate.MemoryMetadata;
                if (metadata is null
                    || !metadata.ArtifactId.Equals(candidate.Artifact.Id)
                    || !metadata.ArtifactRevisionId.Equals(candidate.Revision.Id))
                {
                    return [CreateInvalidTypedItem(candidate, specificity, selectors)];
                }

                return
                [
                    CreateItem(
                        candidate,
                        null,
                        null,
                        candidate.Revision.Content.Value,
                        0,
                        metadata,
                        specificity,
                        selectors,
                        ContextDecisionCodeConstants.Included,
                        "Memory is eligible for deterministic resolution.")
                ];
            }

            return
            [
                CreateItem(
                    candidate,
                    null,
                    null,
                    candidate.Revision.Content.Value,
                    0,
                    null,
                    specificity,
                    selectors,
                    ContextDecisionCodeConstants.ArtifactKindNotContextual,
                    "Document artifacts are available as resources but are not automatically injected into context.")
            ];
        }

        private static ResolvedContextItem CreateInvalidTypedItem(
            ContextCandidateRecord candidate,
            ContextSpecificity specificity,
            IReadOnlyList<ContextSelectorMatch> selectors)
        {
            return CreateItem(
                candidate,
                null,
                null,
                candidate.Revision.Content.Value,
                0,
                candidate.MemoryMetadata,
                specificity,
                selectors,
                ContextDecisionCodeConstants.InvalidTypedGraph,
                "The artifact revision is missing or has inconsistent typed data.");
        }

        private static ResolvedContextItem CreateItem(
            ContextCandidateRecord candidate,
            string? ruleKey,
            string? enforcement,
            string content,
            int rulePriority,
            MemoryMetadata? memoryMetadata,
            ContextSpecificity specificity,
            IReadOnlyList<ContextSelectorMatch> selectors,
            string decisionCode,
            string explanation)
        {
            return new ResolvedContextItem(
                candidate.Binding,
                candidate.Artifact,
                candidate.Revision,
                ruleKey,
                enforcement,
                content,
                rulePriority,
                memoryMetadata,
                specificity,
                selectors,
                Encoding.UTF8.GetByteCount(content),
                decisionCode,
                explanation);
        }

        private static IReadOnlyList<ContextSelectorMatch> MatchSelectors(
            ContextResolutionRequest request,
            Binding binding)
        {
            string? expectedOrganization = binding.OrganizationId?.Value.ToString("D");
            string? actualOrganization = request.Workspace.OrganizationId?.Value.ToString("D");
            string? expectedProject = binding.ProjectId?.Value.ToString("D");
            string? actualProject = request.Project?.Id.Value.ToString("D");
            string? expectedTask = binding.TaskId?.Value.ToString("D");
            string? actualTask = request.Task?.Id.Value.ToString("D");
            string? repository = request.Project?.CanonicalRemoteUri;

            return
            [
                new ContextSelectorMatch(
                    "workspace",
                    binding.WorkspaceId.Value.ToString("D"),
                    request.Workspace.Id.Value.ToString("D"),
                    binding.WorkspaceId.Equals(request.Workspace.Id)),
                new ContextSelectorMatch(
                    "organization",
                    expectedOrganization,
                    actualOrganization,
                    binding.OrganizationId is null
                    || binding.OrganizationId.Equals(request.Workspace.OrganizationId)),
                new ContextSelectorMatch(
                    "project",
                    expectedProject,
                    actualProject,
                    binding.ProjectId is null
                    || binding.ProjectId.Equals(request.Project?.Id)),
                new ContextSelectorMatch(
                    "repository",
                    binding.RepositoryCanonicalUri,
                    repository,
                    binding.RepositoryCanonicalUri is null
                    || binding.RepositoryCanonicalUri.Equals(repository, StringComparison.Ordinal)),
                new ContextSelectorMatch(
                    "path",
                    binding.RepositoryRelativePathPrefix,
                    request.RepositoryRelativePath,
                    PathMatches(
                        binding.RepositoryRelativePathPrefix,
                        request.RepositoryRelativePath)),
                new ContextSelectorMatch(
                    "branch",
                    binding.Branch,
                    request.Branch,
                    binding.Branch is null
                    || binding.Branch.Equals(request.Branch, StringComparison.Ordinal)),
                new ContextSelectorMatch(
                    "task",
                    expectedTask,
                    actualTask,
                    binding.TaskId is null
                    || binding.TaskId.Equals(request.Task?.Id)),
                new ContextSelectorMatch(
                    "agent",
                    binding.Agent,
                    request.Agent,
                    binding.Agent is null
                    || binding.Agent.Equals(request.Agent, StringComparison.OrdinalIgnoreCase))
            ];
        }

        private static bool PathMatches(string? prefix, string? path)
        {
            if (prefix is null)
            {
                return true;
            }

            return path is not null
                   && (path.Equals(prefix, StringComparison.Ordinal)
                       || path.StartsWith($"{prefix}/", StringComparison.Ordinal));
        }

        private static ContextSpecificity CalculateSpecificity(Binding binding)
        {
            string? path = binding.RepositoryRelativePathPrefix;
            int pathSegments = path?.Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
            int pathBytes = path is null ? 0 : Encoding.UTF8.GetByteCount(path);

            return new ContextSpecificity(
                binding.Agent is null ? 0 : 1,
                binding.TaskId is null ? 0 : 1,
                binding.Branch is null ? 0 : 1,
                pathSegments,
                pathBytes,
                binding.RepositoryCanonicalUri is null ? 0 : 1,
                binding.ProjectId is null ? 0 : 1,
                binding.OrganizationId is null ? 0 : 1);
        }

        private static List<ResolvedContextItem> DeduplicateBindings(
            IEnumerable<ResolvedContextItem> eligible,
            ICollection<ResolvedContextItem> excluded)
        {
            List<ResolvedContextItem> selected = [];
            foreach (IGrouping<string, ResolvedContextItem> group in eligible
                         .GroupBy(LogicalItemKey, StringComparer.Ordinal)
                         .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                ResolvedContextItem[] ranked = group
                    .OrderByDescending(item => item.Specificity)
                    .ThenBy(item => item.Binding.Id.Value)
                    .ToArray();
                selected.Add(ranked[0]);

                foreach (ResolvedContextItem redundant in ranked.Skip(1))
                {
                    excluded.Add(WithDecision(
                        redundant,
                        ContextDecisionCodeConstants.RedundantBinding,
                        $"Binding '{ranked[0].Binding.Id.Value:D}' is the most specific binding for this logical item."));
                }
            }

            return selected;
        }

        private static string LogicalItemKey(ResolvedContextItem item)
        {
            string identity = item.MemoryMetadata is null
                ? item.RuleKey ?? string.Empty
                : item.MemoryMetadata.Id.Value.ToString("D");
            return string.Join(
                ':',
                item.Artifact.KindType.Name,
                item.Artifact.Id.Value.ToString("D"),
                item.Revision.Id.Value.ToString("D"),
                identity);
        }

        private static bool IsHardPolicy(ResolvedContextItem item)
        {
            return item.Enforcement?.Equals("hard", StringComparison.Ordinal) == true;
        }

        private static List<ResolvedContextItem> OrderRules(
            IEnumerable<ResolvedContextItem> items)
        {
            return items
                .OrderByDescending(item => item.Specificity)
                .ThenByDescending(item => item.RulePriority)
                .ThenByDescending(item => item.Artifact.Priority.Value)
                .ThenBy(item => item.Artifact.Id.Value)
                .ThenBy(item => item.Revision.Id.Value)
                .ThenBy(item => item.Binding.Id.Value)
                .ThenBy(item => item.RuleKey, StringComparer.Ordinal)
                .ThenBy(item => item.Content, StringComparer.Ordinal)
                .ToList();
        }

        private static List<ResolvedContextItem> OrderMemories(
            IEnumerable<ResolvedContextItem> items)
        {
            return items
                .OrderByDescending(item => item.Specificity)
                .ThenByDescending(item => item.MemoryMetadata!.Confidence)
                .ThenByDescending(item => item.Artifact.Priority.Value)
                .ThenBy(item => item.Artifact.Id.Value)
                .ThenBy(item => item.Revision.Id.Value)
                .ThenBy(item => item.Binding.Id.Value)
                .ToList();
        }

        private static List<ResolvedContextItem> OrderExcluded(
            IEnumerable<ResolvedContextItem> items)
        {
            return items
                .OrderBy(item => item.DecisionCode, StringComparer.Ordinal)
                .ThenBy(item => item.Artifact.Id.Value)
                .ThenBy(item => item.Revision.Id.Value)
                .ThenBy(item => item.Binding.Id.Value)
                .ThenBy(item => item.RuleKey, StringComparer.Ordinal)
                .ToList();
        }

        private static ResolvedContextItem WithDecision(
            ResolvedContextItem item,
            string decisionCode,
            string explanation)
        {
            return item with { DecisionCode = decisionCode, Explanation = explanation };
        }
    }
}