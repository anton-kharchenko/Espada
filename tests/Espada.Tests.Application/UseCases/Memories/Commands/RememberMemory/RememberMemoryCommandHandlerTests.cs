using Espada.Application.UseCases.Memories.Commands.RememberMemory;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Tests.Application.Fixtures;

namespace Espada.Tests.Application.UseCases.Memories.Commands.RememberMemory
{
    public sealed class RememberMemoryCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldCreateUnconfirmedMemoryWithProvenance()
        {
            RememberMemoryHandlerFixture fixture = new();
            Workspace workspace = fixture.GivenWorkspaceExists();
            RememberMemoryCommandHandler handler = fixture.CreateHandler();
            RememberMemoryCommand command = CreateCommand(workspace.Id.Value);

            DomainResult<RememberMemoryResponse> result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            RememberMemoryResponse response = result.ShouldSucceed();
            MemoryMetadata metadata = fixture.MemoryMetadataRepository.AddedMetadata!;
            response.UserConfirmed.Should().BeFalse();
            metadata.UserConfirmed.Should().BeFalse();
            metadata.ClientIdentity.Should().Be("codex");
            metadata.SessionIdentity.Should().Be("session-1");
            metadata.CategoryType.Should().Be(MemoryCategoryType.Decision);
            Binding binding = fixture.BindingRepository.UpsertedBinding!;
            binding.WorkspaceId.Should().Be(workspace.Id);
            binding.ArtifactRevisionId.Should().Be(metadata.ArtifactRevisionId);
            binding.ProjectId.Should().BeNull();
            binding.RepositoryCanonicalUri.Should().BeNull();
            binding.RepositoryRelativePathPrefix.Should().BeNull();
            binding.Branch.Should().BeNull();
            binding.TaskId.Should().BeNull();
            binding.Agent.Should().BeNull();
            fixture.UnitOfWork.SaveChangesCallCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ShouldForwardCancellationTokenToAllWrites()
        {
            RememberMemoryHandlerFixture fixture = new();
            Workspace workspace = fixture.GivenWorkspaceExists();
            RememberMemoryCommandHandler handler = fixture.CreateHandler();
            using CancellationTokenSource source = new();
            CancellationToken cancellationToken = source.Token;

            DomainResult<RememberMemoryResponse> result = await handler.Handle(
                CreateCommand(workspace.Id.Value),
                cancellationToken);

            result.ShouldSucceed();
            fixture.ArtifactRepository.AddCancellationToken.Should().Be(cancellationToken);
            fixture.ArtifactRevisionRepository.AddCancellationToken.Should().Be(cancellationToken);
            fixture.MemoryMetadataRepository.AddCancellationToken.Should().Be(cancellationToken);
            fixture.BindingRepository.UpsertCancellationToken.Should().Be(
                cancellationToken);
            fixture.UnitOfWork.ReceivedCancellationToken.Should().Be(cancellationToken);
        }

        [Fact]
        public async Task Handle_WithoutDefaultEmbeddingModel_ShouldPersistSearchableChunksWithoutVectors()
        {
            RememberMemoryHandlerFixture fixture = new();
            Workspace workspace = fixture.GivenWorkspaceExists();
            fixture.EmbeddingModelDefaults.DefaultModel = null;
            RememberMemoryCommandHandler handler = fixture.CreateHandler();

            DomainResult<RememberMemoryResponse> result = await handler.Handle(
                CreateCommand(workspace.Id.Value),
                TestContext.Current.CancellationToken);

            result.ShouldSucceed();
            fixture.ChunkRepository.AddedChunks.Should().ContainSingle();
            fixture.ChunkEmbeddingRepository.AddedEmbedding.Should().BeNull();
            fixture.UnitOfWork.SaveChangesCallCount.Should().Be(1);
        }

        private static RememberMemoryCommand CreateCommand(Guid workspaceId)
        {
            return new RememberMemoryCommand(
                workspaceId,
                "Repository decision",
                "Use PostgreSQL.",
                MemoryCategoryType.Decision.Id,
                0.8m,
                "codex",
                "session-1");
        }
    }
}