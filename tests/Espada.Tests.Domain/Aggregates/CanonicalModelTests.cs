using Espada.Domain.Errors;
using System.Text.Json;

namespace Espada.Tests.Domain.Aggregates
{
    public sealed class CanonicalModelTests
    {
        public static TheoryData<ArtifactKindType, string> CanonicalKinds => new()
        {
            { ArtifactKindType.Document, "document" },
            { ArtifactKindType.Instruction, "instruction" },
            { ArtifactKindType.Policy, "policy" },
            { ArtifactKindType.Memory, "memory" }
        };

        [Theory]
        [MemberData(nameof(CanonicalKinds))]
        public void ArtifactKind_ShouldRoundTripAsCanonicalJsonString(ArtifactKindType kindType, string identifier)
        {
            string json = JsonSerializer.Serialize(kindType);

            Assert.Equal($"\"{identifier}\"", json);
            Assert.Equal(kindType, JsonSerializer.Deserialize<ArtifactKindType>(json));
        }

        [Fact]
        public void ArtifactKind_ShouldRejectUnknownJsonIdentifier()
        {
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ArtifactKindType>("\"unknown\""));
        }

        [Fact]
        public void ArtifactType_ShouldRemainIndependentContentFormat()
        {
            Artifact artifact = CreateArtifact(ArtifactKindType.Policy, ArtifactType.Markdown);

            Assert.Equal(ArtifactKindType.Policy, artifact.KindType);
            Assert.Equal(ArtifactType.Markdown, artifact.Type);
        }

        [Fact]
        public void Project_Create_ShouldNormalizeAliasesAndCreateTaskInItsWorkspace()
        {
            WorkspaceId workspaceId = WorkspaceId.New();
            Project project = Project.Create(ProjectId.New(), workspaceId, " Espada ",
                " git@github.com:anton/espada.git ", [" C:\\Espada ", "C:\\Espada"], DateTimeOffset.UtcNow).Value;
            ProjectTask task = project.CreateTask(TaskId.New(), "Implement MCP runtime", DateTimeOffset.UtcNow).Value;

            Assert.Equal("Espada", project.Name);
            Assert.Equal(["C:\\Espada"], project.LocalAliases);
            Assert.Equal(workspaceId, task.WorkspaceId);
            Assert.Equal(project.Id, task.ProjectId);
        }

        [Fact]
        public void Task_ShouldFollowActiveCompletedArchivedLifecycle()
        {
            DateTimeOffset createdAtUtc = DateTimeOffset.UtcNow;
            Project project = CreateProject(WorkspaceId.New());
            ProjectTask task = project.CreateTask(TaskId.New(), "Implement MCP runtime", createdAtUtc).Value;

            Assert.Equal("active", task.Status.Name);
            Assert.True(task.Complete(createdAtUtc.AddMinutes(1)).IsSuccess);
            Assert.Equal("completed", task.Status.Name);
            Assert.Equal(TaskErrors.NotActive, task.Complete(createdAtUtc.AddMinutes(2)).Error);
            Assert.True(task.Archive(createdAtUtc.AddMinutes(3)).IsSuccess);
            Assert.Equal("archived", task.Status.Name);
        }

        [Fact]
        public void Binding_ShouldRejectCrossWorkspaceProjectAndTaskSelectors()
        {
            WorkspaceId workspaceId = WorkspaceId.New();
            Workspace workspace = CreateWorkspace(workspaceId);
            Artifact artifact = CreateArtifact(ArtifactKindType.Document, ArtifactType.Text, workspaceId);
            ArtifactRevision revision = CreateRevision(artifact);
            Project foreignProject = CreateProject(WorkspaceId.New());
            Project selectedProject = CreateProject(workspaceId);
            Project otherProject = CreateProject(workspaceId);
            ProjectTask foreignWorkspaceTask =
                foreignProject.CreateTask(TaskId.New(), "Foreign", DateTimeOffset.UtcNow).Value;
            ProjectTask otherProjectTask = otherProject.CreateTask(TaskId.New(), "Other", DateTimeOffset.UtcNow).Value;

            Assert.Equal(BindingErrors.ProjectWorkspaceMismatch,
                artifact.CreateBinding(BindingId.New(), revision, workspace, null, foreignProject, null, null, null,
                    null, null, DateTimeOffset.UtcNow).Error);
            Assert.Equal(BindingErrors.TaskRequiresProject,
                artifact.CreateBinding(BindingId.New(), revision, workspace, null, null, null, null, null,
                    foreignWorkspaceTask, null, DateTimeOffset.UtcNow).Error);
            Assert.Equal(BindingErrors.TaskWorkspaceMismatch,
                artifact.CreateBinding(BindingId.New(), revision, workspace, null, selectedProject, null, null, null,
                    foreignWorkspaceTask, null, DateTimeOffset.UtcNow).Error);
            Assert.Equal(BindingErrors.TaskProjectMismatch,
                artifact.CreateBinding(BindingId.New(), revision, workspace, null, selectedProject, null, null, null,
                    otherProjectTask, null, DateTimeOffset.UtcNow).Error);
        }

        [Fact]
        public void Binding_ShouldValidateSelectorsAgainstDatabaseLengthsAndPathRules()
        {
            Artifact artifact = CreateArtifact(ArtifactKindType.Document);
            ArtifactRevision revision = CreateRevision(artifact);
            Workspace workspace = CreateWorkspace(artifact.WorkspaceId);

            Assert.Equal(BindingErrors.RepositoryCanonicalUriTooLong,
                artifact.CreateBinding(BindingId.New(), revision, workspace, null, null,
                    new string('u', Binding.RepositoryCanonicalUriMaxLength + 1), null, null, null, null,
                    DateTimeOffset.UtcNow).Error);
            Assert.Equal(BindingErrors.RepositoryRelativePathTooLong,
                artifact.CreateBinding(BindingId.New(), revision, workspace, null, null, null,
                    new string('p', Binding.RepositoryRelativePathPrefixMaxLength + 1), null, null, null,
                    DateTimeOffset.UtcNow).Error);
            Assert.Equal(BindingErrors.BranchTooLong,
                artifact.CreateBinding(BindingId.New(), revision, workspace, null, null, null, null,
                    new string('b', Binding.BranchMaxLength + 1), null, null, DateTimeOffset.UtcNow).Error);
            Assert.Equal(BindingErrors.AgentTooLong,
                artifact.CreateBinding(BindingId.New(), revision, workspace, null, null, null, null, null, null,
                    new string('a', Binding.AgentMaxLength + 1), DateTimeOffset.UtcNow).Error);
            Assert.Equal(BindingErrors.RepositoryRelativePathInvalid,
                artifact.CreateBinding(BindingId.New(), revision, workspace, null, null, null, "src/../secrets", null,
                    null, null, DateTimeOffset.UtcNow).Error);
        }

        [Fact]
        public void Rules_ShouldRequireMatchingArtifactKindAndOwnedRevision()
        {
            Artifact instruction = CreateArtifact(ArtifactKindType.Instruction);
            Artifact policy = CreateArtifact(ArtifactKindType.Policy);
            ArtifactRevision instructionRevision = CreateRevision(instruction);
            ArtifactRevision policyRevision = CreateRevision(policy);
            RuleKey key = RuleKey.Create("security.no-secrets").Value;

            InstructionRule instructionRule = instruction
                .CreateInstructionRule(instructionRevision, key, "Use instructions.", ContextPriority.Neutral).Value;
            PolicyRule policyRule = policy.CreatePolicyRule(policyRevision, key, "Never expose secrets.",
                ContextPriority.Create(100).Value, PolicyEnforcementType.Hard).Value;

            Assert.Equal(ArtifactKindType.Instruction, instructionRule.KindType);
            Assert.Equal(ArtifactKindType.Policy, policyRule.KindType);
            Assert.Equal(RuleErrors.PolicyKindRequired,
                instruction.CreatePolicyRule(instructionRevision, key, "Wrong table.", ContextPriority.Neutral,
                    PolicyEnforcementType.Hard).Error);
            Assert.Equal(RuleErrors.InstructionKindRequired,
                policy.CreateInstructionRule(policyRevision, key, "Wrong table.", ContextPriority.Neutral).Error);
            Assert.Equal(RuleErrors.RevisionMismatch,
                instruction.CreateInstructionRule(CreateRevision(CreateArtifact(ArtifactKindType.Instruction)), key,
                    "Foreign revision.", ContextPriority.Neutral).Error);
        }

        [Fact]
        public void MemoryMetadata_ShouldRequireMemoryArtifactOwnedRevisionAndValidateValues()
        {
            Artifact memory = CreateArtifact(ArtifactKindType.Memory);
            Artifact document = CreateArtifact(ArtifactKindType.Document);
            ArtifactRevision revision = CreateRevision(memory);
            MemoryId id = MemoryId.New();

            MemoryMetadata metadata = memory.CreateMemoryMetadata(id, revision, MemoryCategoryType.Fact, 0.75m, true,
                "codex", "session-1", DateTimeOffset.UtcNow).Value;

            Assert.Equal(memory.Id, metadata.ArtifactId);
            Assert.Equal(revision.Id, metadata.ArtifactRevisionId);
            Assert.Equal(ArtifactKindType.Memory, metadata.KindType);
            Assert.Equal(MemoryErrors.ConfidenceOutOfRange,
                memory.CreateMemoryMetadata(MemoryId.New(), revision, MemoryCategoryType.Fact, 1.01m, false, "codex",
                    null, DateTimeOffset.UtcNow).Error);
            Assert.Equal(MemoryErrors.SupersedesSelf,
                memory.CreateMemoryMetadata(id, revision, MemoryCategoryType.Fact, 1m, true, "codex", null,
                    DateTimeOffset.UtcNow, id).Error);
            Assert.Equal(MemoryErrors.MemoryKindRequired,
                document.CreateMemoryMetadata(MemoryId.New(), revision, MemoryCategoryType.Fact, 1m, true, "codex",
                    null, DateTimeOffset.UtcNow).Error);
            Assert.Equal(MemoryErrors.RevisionMismatch,
                memory.CreateMemoryMetadata(MemoryId.New(), CreateRevision(CreateArtifact(ArtifactKindType.Memory)),
                    MemoryCategoryType.Fact, 1m, true, "codex", null, DateTimeOffset.UtcNow).Error);
        }

        [Fact]
        public void OrganizationMembership_ShouldValidateIssuerAndSubjectAgainstDatabaseLengths()
        {
            Organization organization =
                Organization.Create(OrganizationId.New(), "Espada", DateTimeOffset.UtcNow).Value;

            Assert.Equal(OrganizationMembershipErrors.IssuerEmpty,
                organization.CreateMembership(OrganizationMembershipId.New(), " ", "subject",
                    OrganizationMembershipRoleType.Owner, DateTimeOffset.UtcNow).Error);
            Assert.Equal(OrganizationMembershipErrors.SubjectEmpty,
                organization.CreateMembership(OrganizationMembershipId.New(), "issuer", " ",
                    OrganizationMembershipRoleType.Owner, DateTimeOffset.UtcNow).Error);
            Assert.Equal(OrganizationMembershipErrors.IssuerTooLong,
                organization.CreateMembership(OrganizationMembershipId.New(),
                    new string('i', OrganizationMembership.IssuerMaxLength + 1), "subject",
                    OrganizationMembershipRoleType.Owner, DateTimeOffset.UtcNow).Error);
            Assert.Equal(OrganizationMembershipErrors.SubjectTooLong,
                organization.CreateMembership(OrganizationMembershipId.New(), "issuer",
                    new string('s', OrganizationMembership.SubjectMaxLength + 1), OrganizationMembershipRoleType.Owner,
                    DateTimeOffset.UtcNow).Error);
        }

        [Fact]
        public void CanonicalIdentifiers_ShouldReportReadableEmptyGuidErrors()
        {
            Assert.Equal("Organization ID cannot be empty. (Parameter 'value')",
                Assert.Throws<ArgumentException>(() => OrganizationId.Create(Guid.Empty)).Message);
            Assert.Equal("Organization membership ID cannot be empty. (Parameter 'value')",
                Assert.Throws<ArgumentException>(() => OrganizationMembershipId.Create(Guid.Empty)).Message);
            Assert.Equal("Project ID cannot be empty. (Parameter 'value')",
                Assert.Throws<ArgumentException>(() => ProjectId.Create(Guid.Empty)).Message);
            Assert.Equal("Task ID cannot be empty. (Parameter 'value')",
                Assert.Throws<ArgumentException>(() => TaskId.Create(Guid.Empty)).Message);
            Assert.Equal("Binding ID cannot be empty. (Parameter 'value')",
                Assert.Throws<ArgumentException>(() => BindingId.Create(Guid.Empty)).Message);
            Assert.Equal("Memory ID cannot be empty. (Parameter 'value')",
                Assert.Throws<ArgumentException>(() => MemoryId.Create(Guid.Empty)).Message);
        }

        private static Artifact CreateArtifact(ArtifactKindType kindType, ArtifactType? type = null,
            WorkspaceId? workspaceId = null)
        {
            return Artifact.Create(ArtifactId.New(), workspaceId ?? WorkspaceId.New(),
                ArtifactTitle.Create("Canonical artifact").Value, kindType, type ?? ArtifactType.Text,
                DateTimeOffset.UtcNow).Value;
        }

        private static ArtifactRevision CreateRevision(Artifact artifact)
        {
            return artifact.CreateRevision(ArtifactRevisionId.New(), ArtifactContent.Create("canonical content").Value,
                DateTimeOffset.UtcNow).Value;
        }

        private static Project CreateProject(WorkspaceId workspaceId)
        {
            return Project.Create(ProjectId.New(), workspaceId, "Espada",
                $"https://example.test/{Guid.NewGuid():N}.git", [], DateTimeOffset.UtcNow).Value;
        }

        private static Workspace CreateWorkspace(WorkspaceId workspaceId, OrganizationId? organizationId = null)
        {
            return Workspace.Create(
                workspaceId,
                WorkspaceName.Create("Canonical workspace").Value,
                WorkspaceType.Personal,
                organizationId,
                DateTimeOffset.UtcNow).Value;
        }
        [Fact]
        public void Project_Create_WithoutRemote_ShouldCreateLocalOnlyProject()
        {
            Project project = Project.Create(
                ProjectId.New(),
                WorkspaceId.New(),
                "Local project",
                null,
                ["C:\\src\\local"],
                DateTimeOffset.UtcNow).ShouldSucceed();

            Assert.Null(project.CanonicalRemoteUri);
        }

    }
}