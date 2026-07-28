using Espada.Db.Database;
using Espada.Db.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Espada.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Espada.Tests.Integration.Database
{
    [Collection(PostgreSqlIntegrationCollection.Name)]
    public sealed class AggregateRoundTripTests(PostgreSqlDatabaseFixture fixture) : PostgreSqlIntegrationTest(fixture)
    {
        [Fact]
        public async Task CanonicalAggregateGraph_ShouldRoundTripThroughRuntimeModel()
        {
            DateTimeOffset createdAtUtc = new(2026, 7, 28, 5, 0, 0, TimeSpan.Zero);
            Organization organization =
                Organization.Create(OrganizationId.New(), "Espada", createdAtUtc).ShouldSucceed();
            OrganizationMembership membership = organization.CreateMembership(OrganizationMembershipId.New(),
                    "https://issuer.example", "user-123", OrganizationMembershipRoleType.Owner, createdAtUtc)
                .ShouldSucceed();
            Workspace workspace = Workspace.Create(WorkspaceId.New(),
                WorkspaceName.Create("Canonical workspace").ShouldSucceed(), WorkspaceType.Personal, organization.Id,
                createdAtUtc).ShouldSucceed();
            Project project = Project.Create(ProjectId.New(), workspace.Id, "Espada", "https://example.test/espada.git",
                ["C:\\Startups\\Espada"], createdAtUtc).ShouldSucceed();
            ProjectTask task = project.CreateTask(TaskId.New(), "Implement MCP runtime", createdAtUtc).ShouldSucceed();

            Artifact instruction =
                CreateCanonicalArtifact(workspace.Id, ArtifactKindType.Instruction, "Instruction", createdAtUtc);
            ArtifactRevision instructionRevision = instruction.CreateRevision(ArtifactRevisionId.New(),
                ArtifactContent.Create("Use repository instructions.").ShouldSucceed(), createdAtUtc).ShouldSucceed();
            InstructionRule instructionRule = instruction.CreateInstructionRule(instructionRevision,
                RuleKey.Create("repo.instructions").ShouldSucceed(), "Use repository instructions.",
                ContextPriority.Create(10).ShouldSucceed()).ShouldSucceed();
            Binding binding = instruction.CreateBinding(BindingId.New(), instructionRevision, workspace,
                organization.Id, project, project.CanonicalRemoteUri, "src", "feature/canonical-context-runtime", task,
                "codex", createdAtUtc).ShouldSucceed();

            Artifact policy = CreateCanonicalArtifact(workspace.Id, ArtifactKindType.Policy, "Policy", createdAtUtc);
            ArtifactRevision policyRevision = policy.CreateRevision(ArtifactRevisionId.New(),
                ArtifactContent.Create("Never expose secrets.").ShouldSucceed(), createdAtUtc).ShouldSucceed();
            PolicyRule policyRule = policy.CreatePolicyRule(policyRevision,
                RuleKey.Create("security.no-secrets").ShouldSucceed(), "Never expose secrets.",
                ContextPriority.Create(100).ShouldSucceed(), PolicyEnforcementType.Hard).ShouldSucceed();

            Artifact memory = CreateCanonicalArtifact(workspace.Id, ArtifactKindType.Memory, "Memory", createdAtUtc);
            ArtifactRevision memoryRevision = memory.CreateRevision(ArtifactRevisionId.New(),
                    ArtifactContent.Create("The user prefers targeted verification.").ShouldSucceed(), createdAtUtc)
                .ShouldSucceed();
            MemoryMetadata memoryMetadata = memory.CreateMemoryMetadata(MemoryId.New(), memoryRevision,
                MemoryCategoryType.Preference, 0.9m, true, "codex", "session-canonical", createdAtUtc).ShouldSucceed();

            await using (EspadaDbContext dbContext = Fixture.CreateDbContext())
            {
                dbContext.AddRange(organization, membership, workspace, project, task, instruction, instructionRevision,
                    instructionRule, binding, policy, policyRevision, policyRule, memory, memoryRevision,
                    memoryMetadata);
                await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            await using EspadaDbContext verification = Fixture.CreateDbContext();
            Assert.Equal("Espada",
                (await verification.Organizations.FindAsync([organization.Id], TestContext.Current.CancellationToken))!
                .Name);
            Assert.Equal("user-123",
                (await verification.OrganizationMemberships.FindAsync([membership.Id],
                    TestContext.Current.CancellationToken))!.Subject);
            Assert.Equal(workspace.Id,
                (await verification.Projects.FindAsync([project.Id], TestContext.Current.CancellationToken))!
                .WorkspaceId);
            Assert.Equal(project.Id,
                (await verification.Tasks.FindAsync([task.Id], TestContext.Current.CancellationToken))!.ProjectId);
            Assert.Equal(ArtifactKindType.Instruction,
                (await verification.ArtifactRevisions.FindAsync([instructionRevision.Id],
                    TestContext.Current.CancellationToken))!.KindType);
            Assert.Equal(ArtifactKindType.Instruction,
                (await verification.InstructionRules.FindAsync([instructionRevision.Id, instructionRule.RuleKey],
                    TestContext.Current.CancellationToken))!.KindType);
            Assert.Equal(task.Id,
                (await verification.Bindings.FindAsync([binding.Id], TestContext.Current.CancellationToken))!.TaskId);
            Assert.Equal(ArtifactKindType.Policy,
                (await verification.PolicyRules.FindAsync([policyRevision.Id, policyRule.RuleKey],
                    TestContext.Current.CancellationToken))!.KindType);
            MemoryMetadata persistedMemory = Assert.IsType<MemoryMetadata>(
                await verification.MemoryMetadata.FindAsync([memoryMetadata.Id],
                    TestContext.Current.CancellationToken));
            Assert.Equal(memory.Id, persistedMemory.ArtifactId);
            Assert.Equal(memoryRevision.Id, persistedMemory.ArtifactRevisionId);
            Assert.Equal(ArtifactKindType.Memory, persistedMemory.KindType);
        }

        [Fact]
        public async Task CompositeConstraints_ShouldRejectCrossWorkspaceTaskAndBinding()
        {
            DateTimeOffset createdAtUtc = DateTimeOffset.UtcNow;
            Workspace firstWorkspace = CreateWorkspace("First", createdAtUtc);
            Workspace secondWorkspace = CreateWorkspace("Second", createdAtUtc);
            Project firstProject = Project.Create(ProjectId.New(), firstWorkspace.Id, "First",
                "https://example.test/first.git", [], createdAtUtc).ShouldSucceed();
            Artifact artifact =
                CreateCanonicalArtifact(firstWorkspace.Id, ArtifactKindType.Document, "Document", createdAtUtc);
            ArtifactRevision revision = artifact.CreateRevision(ArtifactRevisionId.New(),
                ArtifactContent.Create("Document content").ShouldSucceed(), createdAtUtc).ShouldSucceed();
            await PersistAsync(firstWorkspace, secondWorkspace, firstProject, artifact, revision);

            await using (SetupDbContext setup = Fixture.CreateSetupDbContext())
            {
                setup.Tasks.Add(new Tasks
                {
                    TaskId = Guid.NewGuid(),
                    WorkspaceId = secondWorkspace.Id.Value,
                    ProjectId = firstProject.Id.Value,
                    Title = "Cross workspace",
                    Status = "active",
                    CreatedAtUtc = createdAtUtc,
                    UpdatedAtUtc = createdAtUtc
                });
                await AssertDatabaseViolationAsync(setup, PostgresErrorCodes.ForeignKeyViolation);
            }

            await using (SetupDbContext setup = Fixture.CreateSetupDbContext())
            {
                setup.Bindings.Add(new Bindings
                {
                    BindingId = Guid.NewGuid(),
                    ArtifactRevisionId = revision.Id.Value,
                    WorkspaceId = secondWorkspace.Id.Value,
                    CreatedAtUtc = createdAtUtc
                });
                await AssertDatabaseViolationAsync(setup, PostgresErrorCodes.ForeignKeyViolation);
            }
        }

        [Fact]
        public async Task CompositeConstraint_ShouldRejectMemoryArtifactRevisionMismatch()
        {
            DateTimeOffset createdAtUtc = DateTimeOffset.UtcNow;
            Workspace workspace = CreateWorkspace("Memory", createdAtUtc);
            Artifact first =
                CreateCanonicalArtifact(workspace.Id, ArtifactKindType.Memory, "First memory", createdAtUtc);
            ArtifactRevision firstRevision = first.CreateRevision(ArtifactRevisionId.New(),
                ArtifactContent.Create("First").ShouldSucceed(), createdAtUtc).ShouldSucceed();
            Artifact second =
                CreateCanonicalArtifact(workspace.Id, ArtifactKindType.Memory, "Second memory", createdAtUtc);
            ArtifactRevision secondRevision = second.CreateRevision(ArtifactRevisionId.New(),
                ArtifactContent.Create("Second").ShouldSucceed(), createdAtUtc).ShouldSucceed();
            await PersistAsync(workspace, first, firstRevision, second, secondRevision);

            await using SetupDbContext setup = Fixture.CreateSetupDbContext();
            setup.MemoryMetadata.Add(new MemoryMetadataRecords
            {
                MemoryId = Guid.NewGuid(),
                ArtifactId = second.Id.Value,
                ArtifactRevisionId = firstRevision.Id.Value,
                Kind = "memory",
                Category = "Fact",
                Confidence = 1m,
                UserConfirmed = true,
                ClientIdentity = "codex",
                CapturedAtUtc = createdAtUtc
            });
            await AssertDatabaseViolationAsync(setup, PostgresErrorCodes.ForeignKeyViolation);
        }

        [Fact]
        public async Task RuleConstraints_ShouldRejectWrongRevisionKindAndDisguisedPolicyRule()
        {
            DateTimeOffset createdAtUtc = DateTimeOffset.UtcNow;
            Workspace workspace = CreateWorkspace("Rules", createdAtUtc);
            Artifact instruction =
                CreateCanonicalArtifact(workspace.Id, ArtifactKindType.Instruction, "Instruction", createdAtUtc);
            ArtifactRevision revision = instruction.CreateRevision(ArtifactRevisionId.New(),
                ArtifactContent.Create("Instruction").ShouldSucceed(), createdAtUtc).ShouldSucceed();
            await PersistAsync(workspace, instruction, revision);

            await using (SetupDbContext setup = Fixture.CreateSetupDbContext())
            {
                setup.PolicyRules.Add(new PolicyRules
                {
                    ArtifactRevisionId = revision.Id.Value,
                    Kind = "policy",
                    RuleKey = "wrong-kind",
                    Text = "Wrong",
                    Priority = 0,
                    Enforcement = "Hard"
                });
                await AssertDatabaseViolationAsync(setup, PostgresErrorCodes.ForeignKeyViolation);
            }

            await using (SetupDbContext setup = Fixture.CreateSetupDbContext())
            {
                setup.PolicyRules.Add(new PolicyRules
                {
                    ArtifactRevisionId = revision.Id.Value,
                    Kind = "instruction",
                    RuleKey = "disguised",
                    Text = "Wrong",
                    Priority = 0,
                    Enforcement = "Hard"
                });
                await AssertDatabaseViolationAsync(setup, PostgresErrorCodes.CheckViolation);
            }
        }

        [Fact]
        public async Task Workspace_ShouldRoundTripAllProperties()
        {
            PersistenceGraph graph = await PersistGraphAsync();
            await using EspadaDbContext dbContext = Fixture.CreateDbContext();

            Workspace actual = Assert.IsType<Workspace>(
                await dbContext.Workspaces.FindAsync([graph.Workspace.Id], TestContext.Current.CancellationToken));

            Assert.Equal(graph.Workspace.Id, actual.Id);
            Assert.Equal(graph.Workspace.Name, actual.Name);
            Assert.Equal(graph.Workspace.Type, actual.Type);
            Assert.Equal(graph.Workspace.Status, actual.Status);
            Assert.Equal(graph.Workspace.CreatedAtUtc, actual.CreatedAtUtc);
            Assert.Equal(graph.Workspace.ArchivedAtUtc, actual.ArchivedAtUtc);
            Assert.Equal(graph.Workspace.Version, actual.Version);
        }

        [Fact]
        public async Task Source_ShouldRoundTripAllProperties()
        {
            PersistenceGraph graph = await PersistGraphAsync();
            await using EspadaDbContext dbContext = Fixture.CreateDbContext();

            Source actual =
                Assert.IsType<Source>(await dbContext.Sources.FindAsync([graph.Source.Id],
                    TestContext.Current.CancellationToken));

            Assert.Equal(graph.Source.Id, actual.Id);
            Assert.Equal(graph.Source.WorkspaceId, actual.WorkspaceId);
            Assert.Equal(graph.Source.Name, actual.Name);
            Assert.Equal(graph.Source.Type, actual.Type);
            Assert.Equal(graph.Source.Locator, actual.Locator);
            Assert.Equal(graph.Source.Status, actual.Status);
            Assert.Equal(graph.Source.CreatedAtUtc, actual.CreatedAtUtc);
            Assert.Equal(graph.Source.UpdatedAtUtc, actual.UpdatedAtUtc);
            Assert.Equal(graph.Source.ArchivedAtUtc, actual.ArchivedAtUtc);
            Assert.Equal(graph.Source.Version, actual.Version);
        }

        [Fact]
        public async Task ImportJob_ShouldRoundTripAllProperties()
        {
            PersistenceGraph graph = await PersistGraphAsync();
            await using EspadaDbContext dbContext = Fixture.CreateDbContext();

            ImportJob actual = Assert.IsType<ImportJob>(
                await dbContext.ImportJobs.FindAsync([graph.ImportJob.Id], TestContext.Current.CancellationToken));

            Assert.Equal(graph.ImportJob.Id, actual.Id);
            Assert.Equal(graph.ImportJob.SourceId, actual.SourceId);
            Assert.Equal(graph.ImportJob.WorkspaceId, actual.WorkspaceId);
            Assert.Equal(graph.ImportJob.Status, actual.Status);
            Assert.Equal(graph.ImportJob.RequestedAtUtc, actual.RequestedAtUtc);
            Assert.Equal(graph.ImportJob.StartedAtUtc, actual.StartedAtUtc);
            Assert.Equal(graph.ImportJob.CompletedAtUtc, actual.CompletedAtUtc);
            Assert.Equal(graph.ImportJob.ArtifactId, actual.ArtifactId);
            Assert.Equal(graph.ImportJob.ArtifactRevisionId, actual.ArtifactRevisionId);
            Assert.Equal(graph.ImportJob.Failure, actual.Failure);
            Assert.Equal(graph.ImportJob.Version, actual.Version);
        }

        [Fact]
        public async Task Artifact_ShouldRoundTripAllProperties()
        {
            PersistenceGraph graph = await PersistGraphAsync();
            await using EspadaDbContext dbContext = Fixture.CreateDbContext();

            Artifact actual = Assert.IsType<Artifact>(
                await dbContext.Artifacts.FindAsync([graph.Artifact.Id], TestContext.Current.CancellationToken));

            Assert.Equal(graph.Artifact.Id, actual.Id);
            Assert.Equal(graph.Artifact.WorkspaceId, actual.WorkspaceId);
            Assert.Equal(graph.Artifact.Title, actual.Title);
            Assert.Equal(graph.Artifact.KindType, actual.KindType);
            Assert.Equal(graph.Artifact.Type, actual.Type);
            Assert.Equal(graph.Artifact.Status, actual.Status);
            Assert.Equal(graph.Artifact.CreatedAtUtc, actual.CreatedAtUtc);
            Assert.Equal(graph.Artifact.CurrentRevisionId, actual.CurrentRevisionId);
            Assert.Equal(graph.Artifact.CurrentRevisionNumber, actual.CurrentRevisionNumber);
            Assert.Equal(graph.Artifact.UpdatedAtUtc, actual.UpdatedAtUtc);
            Assert.Equal(graph.Artifact.ArchivedAtUtc, actual.ArchivedAtUtc);
            Assert.Equal(graph.Artifact.RevisionCount, actual.RevisionCount);
            Assert.Equal(graph.Artifact.Version, actual.Version);
        }

        [Fact]
        public async Task ArtifactRevision_ShouldRoundTripAllProperties()
        {
            PersistenceGraph graph = await PersistGraphAsync();
            await using EspadaDbContext dbContext = Fixture.CreateDbContext();

            ArtifactRevision actual = Assert.IsType<ArtifactRevision>(
                await dbContext.ArtifactRevisions.FindAsync([graph.ArtifactRevision.Id],
                    TestContext.Current.CancellationToken));

            Assert.Equal(graph.ArtifactRevision.Id, actual.Id);
            Assert.Equal(graph.ArtifactRevision.ArtifactId, actual.ArtifactId);
            Assert.Equal(graph.ArtifactRevision.WorkspaceId, actual.WorkspaceId);
            Assert.Equal(graph.ArtifactRevision.KindType, actual.KindType);
            Assert.Equal(graph.ArtifactRevision.Number, actual.Number);
            Assert.Equal(graph.ArtifactRevision.Content, actual.Content);
            Assert.Equal(graph.ArtifactRevision.ContentHash, actual.ContentHash);
            Assert.Equal(graph.ArtifactRevision.SizeInBytes, actual.SizeInBytes);
            Assert.Equal(graph.ArtifactRevision.CreatedAtUtc, actual.CreatedAtUtc);
        }

        [Fact]
        public async Task ChunkBatch_ShouldRoundTripAllProperties()
        {
            PersistenceGraph graph = await PersistGraphAsync();
            await using EspadaDbContext dbContext = Fixture.CreateDbContext();

            ChunkBatch actual = Assert.IsType<ChunkBatch>(
                await dbContext.ChunkBatches.FindAsync([graph.ChunkBatch.Id], TestContext.Current.CancellationToken));

            Assert.Equal(graph.ChunkBatch.Id, actual.Id);
            Assert.Equal(graph.ChunkBatch.WorkspaceId, actual.WorkspaceId);
            Assert.Equal(graph.ChunkBatch.ArtifactId, actual.ArtifactId);
            Assert.Equal(graph.ChunkBatch.ArtifactRevisionId, actual.ArtifactRevisionId);
            Assert.Equal(graph.ChunkBatch.Strategy, actual.Strategy);
            Assert.Equal(graph.ChunkBatch.StrategyVersion, actual.StrategyVersion);
            Assert.Equal(graph.ChunkBatch.Status, actual.Status);
            Assert.Equal(graph.ChunkBatch.RequestedAtUtc, actual.RequestedAtUtc);
            Assert.Equal(graph.ChunkBatch.StartedAtUtc, actual.StartedAtUtc);
            Assert.Equal(graph.ChunkBatch.CompletedAtUtc, actual.CompletedAtUtc);
            Assert.Equal(graph.ChunkBatch.ChunkCount, actual.ChunkCount);
            Assert.Equal(graph.ChunkBatch.FailureReason, actual.FailureReason);
            Assert.Equal(graph.ChunkBatch.Version, actual.Version);
        }

        [Fact]
        public async Task Chunk_ShouldRoundTripAllProperties()
        {
            PersistenceGraph graph = await PersistGraphAsync();
            await using EspadaDbContext dbContext = Fixture.CreateDbContext();

            Chunk actual =
                Assert.IsType<Chunk>(
                    await dbContext.Chunks.FindAsync([graph.Chunk.Id], TestContext.Current.CancellationToken));

            Assert.Equal(graph.Chunk.Id, actual.Id);
            Assert.Equal(graph.Chunk.BatchId, actual.BatchId);
            Assert.Equal(graph.Chunk.WorkspaceId, actual.WorkspaceId);
            Assert.Equal(graph.Chunk.ArtifactId, actual.ArtifactId);
            Assert.Equal(graph.Chunk.ArtifactRevisionId, actual.ArtifactRevisionId);
            Assert.Equal(graph.Chunk.Number, actual.Number);
            Assert.Equal(graph.Chunk.Content, actual.Content);
            Assert.Equal(graph.Chunk.SourceSpan, actual.SourceSpan);
            Assert.Equal(graph.Chunk.Strategy, actual.Strategy);
            Assert.Equal(graph.Chunk.StrategyVersion, actual.StrategyVersion);
            Assert.Equal(graph.Chunk.ContentHash, actual.ContentHash);
            Assert.Equal(graph.Chunk.SizeInBytes, actual.SizeInBytes);
            Assert.Equal(graph.Chunk.CharacterCount, actual.CharacterCount);
            Assert.Equal(graph.Chunk.CreatedAtUtc, actual.CreatedAtUtc);
        }

        [Fact]
        public async Task ChunkEmbedding_ShouldRoundTripAllProperties()
        {
            PersistenceGraph graph = await PersistGraphAsync();
            await using EspadaDbContext dbContext = Fixture.CreateDbContext();

            ChunkEmbedding actual = Assert.IsType<ChunkEmbedding>(
                await dbContext.ChunkEmbeddings.FindAsync([graph.ChunkEmbedding.Id],
                    TestContext.Current.CancellationToken));

            Assert.Equal(graph.ChunkEmbedding.Id, actual.Id);
            Assert.Equal(graph.ChunkEmbedding.WorkspaceId, actual.WorkspaceId);
            Assert.Equal(graph.ChunkEmbedding.ChunkId, actual.ChunkId);
            Assert.Equal(graph.ChunkEmbedding.ChunkContentHash, actual.ChunkContentHash);
            Assert.Equal(graph.ChunkEmbedding.Model, actual.Model);
            Assert.Equal(graph.ChunkEmbedding.Dimensions, actual.Dimensions);
            Assert.Equal(graph.ChunkEmbedding.CreatedAtUtc, actual.CreatedAtUtc);
        }

        [Fact]
        public async Task ForeignKeyConstraint_ShouldRejectMissingWorkspace()
        {
            await using EspadaDbContext dbContext = Fixture.CreateDbContext();

            Source source = Source.Create(SourceId.Create(Guid.NewGuid()), WorkspaceId.Create(Guid.NewGuid()),
                SourceName.Create("Orphan source").ShouldSucceed(), SourceType.WebPage,
                SourceLocator.Create($"https://example.com/{Guid.NewGuid():N}").ShouldSucceed(),
                new DateTimeOffset(2026, 7, 26, 5, 0, 0, TimeSpan.Zero)).ShouldSucceed();

            dbContext.Sources.Add(source);

            await AssertDatabaseViolationAsync(dbContext, PostgresErrorCodes.ForeignKeyViolation);
        }

        [Fact]
        public async Task SourceWorkspaceLocatorUniqueConstraint_ShouldRejectDuplicate()
        {
            PersistenceGraph graph = await PersistGraphAsync();
            await using EspadaDbContext dbContext = Fixture.CreateDbContext();

            Source duplicate = Source.Create(SourceId.Create(Guid.NewGuid()), graph.Source.WorkspaceId,
                SourceName.Create("Duplicate source").ShouldSucceed(), graph.Source.Type, graph.Source.Locator,
                graph.Source.CreatedAtUtc).ShouldSucceed();

            dbContext.Sources.Add(duplicate);

            await AssertDatabaseViolationAsync(dbContext, PostgresErrorCodes.UniqueViolation);
        }

        [Fact]
        public async Task ArtifactRevisionArtifactNumberUniqueConstraint_ShouldRejectDuplicate()
        {
            PersistenceGraph graph = await PersistGraphAsync();
            await using EspadaDbContext dbContext = Fixture.CreateDbContext();

            Artifact revisionOwner = Artifact.Create(graph.Artifact.Id, graph.Artifact.WorkspaceId,
                    graph.Artifact.Title, ArtifactKindType.Document, graph.Artifact.Type, graph.Artifact.CreatedAtUtc)
                .ShouldSucceed();

            ArtifactRevision duplicate = revisionOwner.CreateRevision(ArtifactRevisionId.Create(Guid.NewGuid()),
                graph.ArtifactRevision.Content, graph.ArtifactRevision.CreatedAtUtc).ShouldSucceed();

            dbContext.ArtifactRevisions.Add(duplicate);

            await AssertDatabaseViolationAsync(dbContext, PostgresErrorCodes.UniqueViolation);
        }

        [Fact]
        public async Task ChunkBatchIdNumberUniqueConstraint_ShouldRejectDuplicate()
        {
            PersistenceGraph graph = await PersistGraphAsync();
            await using EspadaDbContext dbContext = Fixture.CreateDbContext();

            Chunk duplicate = Chunk.Create(ChunkId.Create(Guid.NewGuid()), graph.Chunk.BatchId, graph.Chunk.WorkspaceId,
                    graph.Chunk.ArtifactId, graph.Chunk.ArtifactRevisionId, graph.Chunk.Number, graph.Chunk.Content,
                    graph.Chunk.SourceSpan, graph.Chunk.Strategy, graph.Chunk.StrategyVersion, graph.Chunk.CreatedAtUtc)
                .ShouldSucceed();

            dbContext.Chunks.Add(duplicate);

            await AssertDatabaseViolationAsync(dbContext, PostgresErrorCodes.UniqueViolation);
        }

        [Fact]
        public async Task ChunkEmbeddingChunkModelUniqueConstraint_ShouldRejectDuplicate()
        {
            PersistenceGraph graph = await PersistGraphAsync();
            await using EspadaDbContext dbContext = Fixture.CreateDbContext();

            ChunkEmbedding duplicate = ChunkEmbedding.Create(ChunkEmbeddingId.Create(Guid.NewGuid()),
                    graph.ChunkEmbedding.WorkspaceId, graph.ChunkEmbedding.ChunkId,
                    graph.ChunkEmbedding.ChunkContentHash,
                    graph.ChunkEmbedding.Model, graph.ChunkEmbedding.Dimensions, graph.ChunkEmbedding.CreatedAtUtc)
                .ShouldSucceed();

            dbContext.ChunkEmbeddings.Add(duplicate);

            await AssertDatabaseViolationAsync(dbContext, PostgresErrorCodes.UniqueViolation);
        }

        [Fact]
        public async Task ConcurrentWorkspaceUpdates_ShouldThrowConcurrencyException()
        {
            PersistenceGraph graph = await PersistGraphAsync();
            await using EspadaDbContext firstContext = Fixture.CreateDbContext();
            await using EspadaDbContext secondContext = Fixture.CreateDbContext();

            Workspace first = Assert.IsType<Workspace>(
                await firstContext.Workspaces.FindAsync([graph.Workspace.Id], TestContext.Current.CancellationToken));
            Workspace second = Assert.IsType<Workspace>(
                await secondContext.Workspaces.FindAsync([graph.Workspace.Id], TestContext.Current.CancellationToken));
            uint originalVersion = first.Version;

            DateTimeOffset firstArchiveTime = new(2026, 7, 26, 6, 0, 0, TimeSpan.Zero);
            first.Archive(firstArchiveTime).ShouldSucceed();
            second.Archive(firstArchiveTime.AddMinutes(1)).ShouldSucceed();

            await firstContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                secondContext.SaveChangesAsync(TestContext.Current.CancellationToken));

            Assert.NotEqual(originalVersion, first.Version);

            await using EspadaDbContext verificationContext = Fixture.CreateDbContext();
            Workspace persisted = Assert.IsType<Workspace>(
                await verificationContext.Workspaces.FindAsync([graph.Workspace.Id],
                    TestContext.Current.CancellationToken));

            Assert.Equal(first.Version, persisted.Version);
            Assert.Equal(firstArchiveTime, persisted.ArchivedAtUtc);
        }

        private static Artifact CreateCanonicalArtifact(WorkspaceId workspaceId, ArtifactKindType kindType,
            string title, DateTimeOffset createdAtUtc)
        {
            return Artifact.Create(ArtifactId.New(), workspaceId, ArtifactTitle.Create(title).ShouldSucceed(), kindType,
                ArtifactType.Markdown, createdAtUtc).ShouldSucceed();
        }

        private static Workspace CreateWorkspace(string name, DateTimeOffset createdAtUtc)
        {
            return Workspace.Create(WorkspaceId.New(), WorkspaceName.Create(name).ShouldSucceed(),
                WorkspaceType.Personal, null, createdAtUtc).ShouldSucceed();
        }

        private async Task PersistAsync(params object[] entities)
        {
            await using EspadaDbContext dbContext = Fixture.CreateDbContext();
            dbContext.AddRange(entities);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        private async Task<PersistenceGraph> PersistGraphAsync()
        {
            PersistenceGraph graph = PersistenceGraphFactory.Create();
            await using EspadaDbContext dbContext = Fixture.CreateDbContext();

            dbContext.AddRange(graph.Workspace, graph.Source, graph.ImportJob, graph.Artifact, graph.ArtifactRevision,
                graph.ChunkBatch, graph.Chunk, graph.ChunkEmbedding);

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            dbContext.ChangeTracker.Clear();

            return graph;
        }

        private static async Task AssertDatabaseViolationAsync(EspadaDbContext dbContext, string expectedSqlState)
        {
            DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(() =>
                dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));
            PostgresException postgresException = Assert.IsType<PostgresException>(exception.InnerException);

            Assert.Equal(expectedSqlState, postgresException.SqlState);
        }

        private static async Task AssertDatabaseViolationAsync(SetupDbContext dbContext, string expectedSqlState)
        {
            DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(() =>
                dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));
            PostgresException postgresException = Assert.IsType<PostgresException>(exception.InnerException);

            Assert.Equal(expectedSqlState, postgresException.SqlState);
        }
    }
}