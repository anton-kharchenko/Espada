using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Espada.Tests.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Tests.Integration.Repositories
{
    [Collection(PostgreSqlIntegrationCollection.Name)]
    public sealed class ContextCandidateStoreTests(
        PostgreSqlDatabaseFixture fixture)
        : PostgreSqlIntegrationTest(fixture)
    {
        private static readonly DateTimeOffset CreatedAtUtc =
            new(2026, 7, 28, 16, 0, 0, TimeSpan.Zero);

        [Fact]
        public async Task LoadByWorkspaceIdAsync_ShouldHydrateExactBoundRevision()
        {
            Workspace workspace = CreateWorkspace("Context");
            Workspace foreignWorkspace = CreateWorkspace("Foreign");

            Artifact instruction = CreateArtifact(
                workspace,
                ArtifactKindType.Instruction,
                "Instruction");
            ArtifactRevision firstRevision = CreateRevision(
                instruction,
                "First revision.");
            InstructionRule firstRule = instruction.CreateInstructionRule(
                firstRevision,
                RuleKey.Create("instruction.first").Value,
                "First revision.",
                ContextPriority.Neutral).Value;
            ArtifactRevision currentRevision = CreateRevision(
                instruction,
                "Current revision.");
            InstructionRule currentRule = instruction.CreateInstructionRule(
                currentRevision,
                RuleKey.Create("instruction.current").Value,
                "Current revision.",
                ContextPriority.Neutral).Value;
            Binding instructionBinding = CreateBinding(
                workspace,
                instruction,
                firstRevision);

            Artifact document = CreateArtifact(
                workspace,
                ArtifactKindType.Document,
                "Archived document");
            ArtifactRevision documentRevision = CreateRevision(
                document,
                "Archived document.");
            Binding documentBinding = CreateBinding(
                workspace,
                document,
                documentRevision);
            document.Archive(CreatedAtUtc.AddMinutes(1)).ShouldSucceed();

            Artifact oldMemory = CreateArtifact(
                workspace,
                ArtifactKindType.Memory,
                "Old memory");
            ArtifactRevision oldMemoryRevision = CreateRevision(
                oldMemory,
                "Old memory.");
            MemoryMetadata oldMetadata = oldMemory.CreateMemoryMetadata(
                MemoryId.New(),
                oldMemoryRevision,
                MemoryCategoryType.Fact,
                0.5m,
                false,
                "codex",
                "session-old",
                CreatedAtUtc).Value;
            Binding memoryBinding = CreateBinding(
                workspace,
                oldMemory,
                oldMemoryRevision);

            Artifact newMemory = CreateArtifact(
                workspace,
                ArtifactKindType.Memory,
                "New memory");
            ArtifactRevision newMemoryRevision = CreateRevision(
                newMemory,
                "New memory.");
            MemoryMetadata newMetadata = newMemory.CreateMemoryMetadata(
                MemoryId.New(),
                newMemoryRevision,
                MemoryCategoryType.Fact,
                0.9m,
                false,
                "claude",
                "session-new",
                CreatedAtUtc,
                oldMetadata.Id).Value;

            Artifact foreignArtifact = CreateArtifact(
                foreignWorkspace,
                ArtifactKindType.Document,
                "Foreign document");
            ArtifactRevision foreignRevision = CreateRevision(
                foreignArtifact,
                "Foreign.");
            Binding foreignBinding = CreateBinding(
                foreignWorkspace,
                foreignArtifact,
                foreignRevision);

            await using (EspadaDbContext dbContext = Fixture.CreateDbContext())
            {
                dbContext.AddRange(
                    workspace,
                    foreignWorkspace,
                    instruction,
                    firstRevision,
                    firstRule,
                    currentRevision,
                    currentRule,
                    instructionBinding,
                    document,
                    documentRevision,
                    documentBinding,
                    oldMemory,
                    oldMemoryRevision,
                    oldMetadata,
                    memoryBinding,
                    newMemory,
                    newMemoryRevision,
                    newMetadata,
                    foreignArtifact,
                    foreignRevision,
                    foreignBinding);
                await dbContext.SaveChangesAsync(
                    TestContext.Current.CancellationToken);
            }

            await using ServiceProvider serviceProvider =
                Fixture.CreateServiceProvider();
            await using AsyncServiceScope scope =
                serviceProvider.CreateAsyncScope();
            IContextCandidateStore store = scope.ServiceProvider
                .GetRequiredService<IContextCandidateStore>();

            IReadOnlyList<ContextCandidateRecord> candidates =
                await store.LoadByWorkspaceIdAsync(
                    workspace.Id,
                    TestContext.Current.CancellationToken);

            Assert.Equal(3, candidates.Count);
            ContextCandidateRecord instructionCandidate = candidates.Single(
                candidate => candidate.Binding.Id.Equals(
                    instructionBinding.Id));
            Assert.Equal(firstRevision.Id, instructionCandidate.Revision.Id);
            Assert.Equal(
                "instruction.first",
                Assert.Single(instructionCandidate.InstructionRules)
                    .RuleKey.Value);
            Assert.DoesNotContain(
                instructionCandidate.InstructionRules,
                rule => rule.RuleKey.Value == "instruction.current");

            ContextCandidateRecord documentCandidate = candidates.Single(
                candidate => candidate.Binding.Id.Equals(documentBinding.Id));
            Assert.Equal(
                ArtifactStatusType.Archived,
                documentCandidate.Artifact.Status);

            ContextCandidateRecord memoryCandidate = candidates.Single(
                candidate => candidate.Binding.Id.Equals(memoryBinding.Id));
            Assert.Equal(oldMetadata.Id, memoryCandidate.MemoryMetadata!.Id);
            Assert.True(memoryCandidate.IsMemorySuperseded);
            Assert.DoesNotContain(
                candidates,
                candidate => candidate.Binding.Id.Equals(foreignBinding.Id));
        }

        [Fact]
        public async Task LoadByWorkspaceIdAsync_ShouldObserveCancellation()
        {
            await using ServiceProvider serviceProvider =
                Fixture.CreateServiceProvider();
            await using AsyncServiceScope scope =
                serviceProvider.CreateAsyncScope();
            IContextCandidateStore store = scope.ServiceProvider
                .GetRequiredService<IContextCandidateStore>();
            using CancellationTokenSource source = new();
            source.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => await store.LoadByWorkspaceIdAsync(
                    WorkspaceId.New(),
                    source.Token));
        }

        private static Workspace CreateWorkspace(string name) =>
            Workspace.Create(
                WorkspaceId.New(),
                WorkspaceName.Create(name).Value,
                WorkspaceType.Personal,
                null,
                CreatedAtUtc).Value;

        private static Artifact CreateArtifact(
            Workspace workspace,
            ArtifactKindType kindType,
            string title) =>
            Artifact.Create(
                ArtifactId.New(),
                workspace.Id,
                ArtifactTitle.Create(title).Value,
                kindType,
                ArtifactType.Markdown,
                CreatedAtUtc).Value;

        private static ArtifactRevision CreateRevision(
            Artifact artifact,
            string content) =>
            artifact.CreateRevision(
                ArtifactRevisionId.New(),
                ArtifactContent.Create(content).Value,
                CreatedAtUtc).Value;

        private static Binding CreateBinding(
            Workspace workspace,
            Artifact artifact,
            ArtifactRevision revision) =>
            artifact.CreateBinding(
                BindingId.New(),
                revision,
                workspace,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                CreatedAtUtc).Value;
    }
}
