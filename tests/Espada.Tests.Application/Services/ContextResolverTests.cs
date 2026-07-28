using Espada.Application.ApplicationErrors;
using Espada.Application.Models;
using Espada.Application.Services;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using System.Text;
using Espada.Application.Constants;

namespace Espada.Tests.Application.Services
{
    public sealed class ContextResolverTests
    {
        private static readonly DateTimeOffset CreatedAtUtc =
            new(2026, 7, 28, 12, 0, 0, TimeSpan.Zero);

        private readonly ContextResolver _resolver = new();

        [Fact]
        public void Resolve_WithCompleteSelectorChain_ShouldMatchEverySelector()
        {
            Organization organization = CreateOrganization();
            Workspace workspace = CreateWorkspace(organization.Id);
            Project project = CreateProject(workspace);
            ProjectTask task = project.CreateTask(
                TaskId.New(),
                "Resolver",
                CreatedAtUtc).Value;
            ContextCandidateRecord candidate = CreateInstructionCandidate(
                workspace,
                "scope.complete",
                "Use the complete selector chain.",
                organization.Id,
                project,
                project.CanonicalRemoteUri,
                "src/app",
                "feature/context",
                task,
                "CoDeX");
            ContextResolutionRequest request = new(
                workspace,
                project,
                task,
                "src/app/file.cs",
                "feature/context",
                ContextAgentConstants.Codex,
                4_096);

            ResolvedContext resolved = _resolver.Resolve(
                request,
                [candidate]).ShouldSucceed();

            ResolvedContextItem item = Assert.Single(resolved.IncludedItems);
            Assert.All(item.Selectors, selector => Assert.True(selector.Matched));
            Assert.Equal(new ContextSpecificity(1, 1, 1, 2, 7, 1, 1, 1), item.Specificity);
        }

        [Fact]
        public void Resolve_WithPathBoundaryMismatch_ShouldExplainExclusion()
        {
            Workspace workspace = CreateWorkspace();
            Project project = CreateProject(workspace);
            ContextCandidateRecord candidate = CreateInstructionCandidate(
                workspace,
                "path.boundary",
                "Use path-scoped instructions.",
                project: project,
                repositoryCanonicalUri: project.CanonicalRemoteUri,
                path: "src/app");
            ContextResolutionRequest request = new(
                workspace,
                project,
                null,
                "src/application/file.cs",
                null,
                ContextAgentConstants.Codex,
                4_096);

            ResolvedContext resolved = _resolver.Resolve(
                request,
                [candidate]).ShouldSucceed();

            Assert.Empty(resolved.IncludedItems);
            ResolvedContextItem excluded = Assert.Single(resolved.ExcludedItems);
            Assert.Equal(ContextDecisionCodeConstants.SelectorMismatch, excluded.DecisionCode);
            Assert.False(excluded.Selectors.Single(selector => selector.Selector == "path").Matched);
            Assert.Contains("path", excluded.Explanation, StringComparison.Ordinal);
        }

        [Fact]
        public void Resolve_WithNarrowerSoftRule_ShouldReplaceWiderRuleBeforePriority()
        {
            Workspace workspace = CreateWorkspace();
            Project project = CreateProject(workspace);
            ContextCandidateRecord wider = CreateInstructionCandidate(
                workspace,
                "rule.shared",
                "Wider but high priority.",
                rulePriority: 100);
            ContextCandidateRecord narrower = CreateInstructionCandidate(
                workspace,
                "rule.shared",
                "Project-specific low priority.",
                project: project,
                rulePriority: -100);
            ContextResolutionRequest request = new(
                workspace,
                project,
                null,
                null,
                null,
                ContextAgentConstants.Codex,
                4_096);

            ResolvedContext resolved = _resolver.Resolve(
                request,
                [wider, narrower]).ShouldSucceed();

            Assert.Equal(
                "Project-specific low priority.",
                Assert.Single(resolved.IncludedItems).Content);
            Assert.Equal(
                ContextDecisionCodeConstants.OverriddenSoftRule,
                Assert.Single(resolved.ExcludedItems).DecisionCode);
        }

        [Fact]
        public void Resolve_WithEqualSpecificity_ShouldUseRulePriorityAndReportAmbiguity()
        {
            Workspace workspace = CreateWorkspace();
            ContextCandidateRecord lower = CreateInstructionCandidate(
                workspace,
                "rule.ambiguous",
                "Lower priority.",
                rulePriority: 1);
            ContextCandidateRecord higher = CreateInstructionCandidate(
                workspace,
                "rule.ambiguous",
                "Higher priority.",
                rulePriority: 2);
            ContextResolutionRequest request = CreateRequest(workspace);

            ResolvedContext resolved = _resolver.Resolve(
                request,
                [lower, higher]).ShouldSucceed();

            Assert.Equal(
                "Higher priority.",
                Assert.Single(resolved.IncludedItems).Content);
            ContextConflict conflict = Assert.Single(resolved.Conflicts);
            Assert.Equal(ContextDecisionCodeConstants.AmbiguousSoftRule, conflict.ConflictCode);
            Assert.Equal(higher.Artifact.Id.Value, conflict.WinnerArtifactId);
        }

        [Fact]
        public void Resolve_WithHardPolicy_ShouldBlockSameKeySoftRule()
        {
            Workspace workspace = CreateWorkspace();
            ContextCandidateRecord hard = CreatePolicyCandidate(
                workspace,
                "security.secrets",
                "Never expose secrets.",
                PolicyEnforcementType.Hard);
            ContextCandidateRecord soft = CreateInstructionCandidate(
                workspace,
                "security.secrets",
                "Secrets may be printed.");

            ResolvedContext resolved = _resolver.Resolve(
                CreateRequest(workspace),
                [soft, hard]).ShouldSucceed();

            Assert.Equal(
                hard.Artifact.Id,
                Assert.Single(resolved.IncludedItems).Artifact.Id);
            Assert.Equal(
                ContextDecisionCodeConstants.BlockedByHardPolicy,
                Assert.Single(resolved.ExcludedItems).DecisionCode);
        }

        [Fact]
        public void Resolve_WithDifferentHardPolicyTexts_ShouldRetainAllAndReportConflict()
        {
            Workspace workspace = CreateWorkspace();
            ContextCandidateRecord first = CreatePolicyCandidate(
                workspace,
                "security.review",
                "Require one reviewer.",
                PolicyEnforcementType.Hard);
            ContextCandidateRecord second = CreatePolicyCandidate(
                workspace,
                "security.review",
                "Require two reviewers.",
                PolicyEnforcementType.Hard);

            ResolvedContext resolved = _resolver.Resolve(
                CreateRequest(workspace),
                [second, first]).ShouldSucceed();

            Assert.Equal(2, resolved.IncludedItems.Count);
            Assert.Equal(
                ContextDecisionCodeConstants.HardPolicyConflict,
                Assert.Single(resolved.Conflicts).ConflictCode);
        }

        [Fact]
        public void Resolve_WithDuplicateBindings_ShouldKeepMostSpecificBinding()
        {
            Workspace workspace = CreateWorkspace();
            Project project = CreateProject(workspace);
            ContextCandidateRecord wider = CreateInstructionCandidate(
                workspace,
                "binding.duplicate",
                "One logical rule.");
            Binding narrowerBinding = wider.Artifact.CreateBinding(
                BindingId.New(),
                wider.Revision,
                workspace,
                null,
                project,
                null,
                null,
                null,
                null,
                null,
                CreatedAtUtc).Value;
            ContextCandidateRecord narrower = wider with { Binding = narrowerBinding };
            ContextResolutionRequest request = new(
                workspace,
                project,
                null,
                null,
                null,
                ContextAgentConstants.Codex,
                4_096);

            ResolvedContext resolved = _resolver.Resolve(
                request,
                [wider, narrower]).ShouldSucceed();

            Assert.Equal(
                narrowerBinding.Id,
                Assert.Single(resolved.IncludedItems).Binding.Id);
            Assert.Equal(
                ContextDecisionCodeConstants.RedundantBinding,
                Assert.Single(resolved.ExcludedItems).DecisionCode);
        }

        [Fact]
        public void Resolve_ShouldRankConfirmedMemoryBeforeUnconfirmedMemory()
        {
            Workspace workspace = CreateWorkspace();
            ContextCandidateRecord unconfirmed = CreateMemoryCandidate(
                workspace,
                "Unconfirmed memory.",
                false,
                1m,
                "claude",
                "session-unconfirmed");
            ContextCandidateRecord confirmed = CreateMemoryCandidate(
                workspace,
                "Confirmed memory.",
                true,
                0.1m,
                "codex",
                "session-confirmed");

            ResolvedContext resolved = _resolver.Resolve(
                CreateRequest(workspace),
                [unconfirmed, confirmed]).ShouldSucceed();

            Assert.Equal(
                ["Confirmed memory.", "Unconfirmed memory."],
                resolved.IncludedItems.Select(item => item.Content));
            MemoryMetadata metadata = resolved.IncludedItems[1].MemoryMetadata!;
            Assert.False(metadata.UserConfirmed);
            Assert.Equal("claude", metadata.ClientIdentity);
            Assert.Equal("session-unconfirmed", metadata.SessionIdentity);
            Assert.Equal(1m, metadata.Confidence);
        }

        [Fact]
        public void Resolve_WithSupersededMemory_ShouldExcludeIt()
        {
            Workspace workspace = CreateWorkspace();
            ContextCandidateRecord memory = CreateMemoryCandidate(
                    workspace,
                    "Old memory.",
                    false,
                    0.5m,
                    "codex",
                    null) with
                {
                    IsMemorySuperseded = true
                };

            ResolvedContext resolved = _resolver.Resolve(
                CreateRequest(workspace),
                [memory]).ShouldSucceed();

            Assert.Empty(resolved.IncludedItems);
            Assert.Equal(
                ContextDecisionCodeConstants.SupersededMemory,
                Assert.Single(resolved.ExcludedItems).DecisionCode);
        }

        [Fact]
        public void Resolve_ShouldCountUtf8BytesAndNeverSplitItems()
        {
            Workspace workspace = CreateWorkspace();
            ContextCandidateRecord candidate = CreateInstructionCandidate(
                workspace,
                "budget.utf8",
                "é");

            ResolvedContext exact = _resolver.Resolve(
                CreateRequest(workspace, 2),
                [candidate]).ShouldSucceed();
            ResolvedContext tooSmall = _resolver.Resolve(
                CreateRequest(workspace, 1),
                [candidate]).ShouldSucceed();

            Assert.Equal(2, Assert.Single(exact.IncludedItems).SizeInBytes);
            Assert.Equal(2, exact.Budget.UsedBytes);
            Assert.Empty(tooSmall.IncludedItems);
            ResolvedContextItem excluded = Assert.Single(tooSmall.ExcludedItems);
            Assert.Equal(ContextDecisionCodeConstants.BudgetExceeded, excluded.DecisionCode);
            Assert.Equal("é", excluded.Content);
        }

        [Fact]
        public void Resolve_WithHardPolicyBudget_ShouldSucceedExactlyAndFailOneByteBelow()
        {
            Workspace workspace = CreateWorkspace();
            ContextCandidateRecord hard = CreatePolicyCandidate(
                workspace,
                "budget.hard",
                "Hard policy.",
                PolicyEnforcementType.Hard);
            int size = Encoding.UTF8.GetByteCount("Hard policy.");

            DomainResult<ResolvedContext> exact = _resolver.Resolve(
                CreateRequest(workspace, size),
                [hard]);
            DomainResult<ResolvedContext> tooSmall = _resolver.Resolve(
                CreateRequest(workspace, size - 1),
                [hard]);

            Assert.True(exact.IsSuccess);
            tooSmall.ShouldFailWith(ContextApplicationErrors.BudgetTooSmall);
        }

        [Fact]
        public void Resolve_WhenLargeOptionalItemDoesNotFit_ShouldContinueToSmallerItem()
        {
            Workspace workspace = CreateWorkspace();
            ContextCandidateRecord large = CreateInstructionCandidate(
                workspace,
                "budget.large",
                "This item is too large.",
                rulePriority: 10);
            ContextCandidateRecord small = CreateInstructionCandidate(
                workspace,
                "budget.small",
                "fit",
                rulePriority: 0);

            ResolvedContext resolved = _resolver.Resolve(
                CreateRequest(workspace, 3),
                [small, large]).ShouldSucceed();

            Assert.Equal("fit", Assert.Single(resolved.IncludedItems).Content);
            Assert.Contains(
                resolved.ExcludedItems,
                item => item.Content == "This item is too large."
                        && item.DecisionCode == ContextDecisionCodeConstants.BudgetExceeded);
        }

        [Fact]
        public void Resolve_WithPermutedInput_ShouldProduceIdenticalOrdering()
        {
            Workspace workspace = CreateWorkspace();
            ContextCandidateRecord instruction = CreateInstructionCandidate(
                workspace,
                "order.instruction",
                "Instruction.");
            ContextCandidateRecord policy = CreatePolicyCandidate(
                workspace,
                "order.policy",
                "Policy.",
                PolicyEnforcementType.Hard);
            ContextCandidateRecord memory = CreateMemoryCandidate(
                workspace,
                "Memory.",
                false,
                0.8m,
                "gemini",
                null);
            ContextResolutionRequest request = CreateRequest(workspace);

            ResolvedContext first = _resolver.Resolve(
                request,
                [instruction, policy, memory]).ShouldSucceed();
            ResolvedContext second = _resolver.Resolve(
                request,
                [memory, instruction, policy]).ShouldSucceed();

            Assert.Equal(ItemIdentities(first.IncludedItems), ItemIdentities(second.IncludedItems));
            Assert.Equal(ItemIdentities(first.ExcludedItems), ItemIdentities(second.ExcludedItems));
            Assert.Equal(first.Conflicts, second.Conflicts);
            Assert.Equal(first.Budget, second.Budget);
        }

        [Fact]
        public void Resolve_WithInvalidTypedGraphAndDocument_ShouldExplainBoth()
        {
            Workspace workspace = CreateWorkspace();
            Artifact instruction = CreateArtifact(
                workspace,
                ArtifactKindType.Instruction,
                "Invalid instruction",
                "raw");
            ArtifactRevision revision = CreateRevision(instruction, "raw");
            Binding binding = CreateBinding(workspace, instruction, revision);
            ContextCandidateRecord invalid = new(
                binding,
                instruction,
                revision,
                [],
                [],
                null,
                false);
            ContextCandidateRecord document = CreateDocumentCandidate(workspace);

            ResolvedContext resolved = _resolver.Resolve(
                CreateRequest(workspace),
                [invalid, document]).ShouldSucceed();

            Assert.Contains(
                resolved.ExcludedItems,
                item => item.DecisionCode == ContextDecisionCodeConstants.InvalidTypedGraph);
            Assert.Contains(
                resolved.ExcludedItems,
                item => item.DecisionCode == ContextDecisionCodeConstants.ArtifactKindNotContextual);
            Assert.Contains(
                resolved.Conflicts,
                conflict => conflict.ConflictCode == ContextDecisionCodeConstants.InvalidTypedGraph);
        }

        private static ContextResolutionRequest CreateRequest(
            Workspace workspace,
            int budget = 4_096)
        {
            return new ContextResolutionRequest(
                workspace,
                null,
                null,
                null,
                null,
                ContextAgentConstants.Codex,
                budget);
        }

        private static Organization CreateOrganization()
        {
            return Organization.Create(
                OrganizationId.New(),
                "Espada",
                CreatedAtUtc).Value;
        }

        private static Workspace CreateWorkspace(
            OrganizationId? organizationId = null)
        {
            return Workspace.Create(
                WorkspaceId.New(),
                WorkspaceName.Create("Resolver workspace").Value,
                organizationId is null
                    ? WorkspaceType.Personal
                    : WorkspaceType.Organization,
                organizationId,
                CreatedAtUtc).Value;
        }

        private static Project CreateProject(Workspace workspace)
        {
            return Project.Create(
                ProjectId.New(),
                workspace.Id,
                "Espada",
                $"https://example.test/{Guid.NewGuid():N}.git",
                [],
                CreatedAtUtc).Value;
        }

        private static ContextCandidateRecord CreateInstructionCandidate(
            Workspace workspace,
            string ruleKey,
            string text,
            OrganizationId? organizationId = null,
            Project? project = null,
            string? repositoryCanonicalUri = null,
            string? path = null,
            string? branch = null,
            ProjectTask? task = null,
            string? agent = null,
            int rulePriority = 0)
        {
            Artifact artifact = CreateArtifact(
                workspace,
                ArtifactKindType.Instruction,
                ruleKey,
                text);
            ArtifactRevision revision = CreateRevision(artifact, text);
            InstructionRule rule = artifact.CreateInstructionRule(
                revision,
                RuleKey.Create(ruleKey).Value,
                text,
                ContextPriority.Create(rulePriority).Value).Value;
            Binding binding = CreateBinding(
                workspace,
                artifact,
                revision,
                organizationId,
                project,
                repositoryCanonicalUri,
                path,
                branch,
                task,
                agent);
            return new ContextCandidateRecord(
                binding,
                artifact,
                revision,
                [rule],
                [],
                null,
                false);
        }

        private static ContextCandidateRecord CreatePolicyCandidate(
            Workspace workspace,
            string ruleKey,
            string text,
            PolicyEnforcementType enforcementType)
        {
            Artifact artifact = CreateArtifact(
                workspace,
                ArtifactKindType.Policy,
                ruleKey,
                text);
            ArtifactRevision revision = CreateRevision(artifact, text);
            PolicyRule rule = artifact.CreatePolicyRule(
                revision,
                RuleKey.Create(ruleKey).Value,
                text,
                ContextPriority.Neutral,
                enforcementType).Value;
            Binding binding = CreateBinding(workspace, artifact, revision);
            return new ContextCandidateRecord(
                binding,
                artifact,
                revision,
                [],
                [rule],
                null,
                false);
        }

        private static ContextCandidateRecord CreateMemoryCandidate(
            Workspace workspace,
            string content,
            bool userConfirmed,
            decimal confidence,
            string clientIdentity,
            string? sessionIdentity)
        {
            Artifact artifact = CreateArtifact(
                workspace,
                ArtifactKindType.Memory,
                "Memory",
                content);
            ArtifactRevision revision = CreateRevision(artifact, content);
            MemoryMetadata metadata = artifact.CreateMemoryMetadata(
                MemoryId.New(),
                revision,
                MemoryCategoryType.Fact,
                confidence,
                userConfirmed,
                clientIdentity,
                sessionIdentity,
                CreatedAtUtc).Value;
            Binding binding = CreateBinding(workspace, artifact, revision);
            return new ContextCandidateRecord(
                binding,
                artifact,
                revision,
                [],
                [],
                metadata,
                false);
        }

        private static ContextCandidateRecord CreateDocumentCandidate(
            Workspace workspace)
        {
            Artifact artifact = CreateArtifact(
                workspace,
                ArtifactKindType.Document,
                "Document",
                "Document content.");
            ArtifactRevision revision = CreateRevision(
                artifact,
                "Document content.");
            Binding binding = CreateBinding(workspace, artifact, revision);
            return new ContextCandidateRecord(
                binding,
                artifact,
                revision,
                [],
                [],
                null,
                false);
        }

        private static Artifact CreateArtifact(
            Workspace workspace,
            ArtifactKindType kindType,
            string title,
            string content)
        {
            Artifact artifact = Artifact.Create(
                ArtifactId.New(),
                workspace.Id,
                ArtifactTitle.Create(title).Value,
                kindType,
                ArtifactType.Markdown,
                CreatedAtUtc).Value;
            _ = content;
            return artifact;
        }

        private static ArtifactRevision CreateRevision(
            Artifact artifact,
            string content)
        {
            return artifact.CreateRevision(
                ArtifactRevisionId.New(),
                ArtifactContent.Create(content).Value,
                CreatedAtUtc).Value;
        }

        private static Binding CreateBinding(
            Workspace workspace,
            Artifact artifact,
            ArtifactRevision revision,
            OrganizationId? organizationId = null,
            Project? project = null,
            string? repositoryCanonicalUri = null,
            string? path = null,
            string? branch = null,
            ProjectTask? task = null,
            string? agent = null)
        {
            return artifact.CreateBinding(
                BindingId.New(),
                revision,
                workspace,
                organizationId,
                project,
                repositoryCanonicalUri,
                path,
                branch,
                task,
                agent,
                CreatedAtUtc).Value;
        }

        private static string[] ItemIdentities(
            IEnumerable<ResolvedContextItem> items)
        {
            return items
                .Select(item => string.Join(
                    ':',
                    item.Binding.Id.Value,
                    item.Artifact.Id.Value,
                    item.Revision.Id.Value,
                    item.RuleKey,
                    item.DecisionCode))
                .ToArray();
        }
    }
}