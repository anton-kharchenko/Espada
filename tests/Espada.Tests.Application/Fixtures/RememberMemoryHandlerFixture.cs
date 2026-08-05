using AutoMapper;
using Espada.Application.Mappings;
using Espada.Application.Services;
using Espada.Application.Services.Billing;
using Espada.Application.UseCases.Memories.Commands.RememberMemory;
using Espada.Domain.Aggregates;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;
using Microsoft.Extensions.Logging.Abstractions;

namespace Espada.Tests.Application.Fixtures
{
    internal sealed class RememberMemoryHandlerFixture
    {
        public WorkspaceRepositorySpy WorkspaceRepository { get; } = new();
        public ArtifactRepositorySpy ArtifactRepository { get; } = new();
        public ArtifactRevisionRepositorySpy ArtifactRevisionRepository { get; } = new();
        public MemoryMetadataRepositorySpy MemoryMetadataRepository { get; } = new();
        public BindingRepositorySpy BindingRepository { get; } = new();
        public ChunkBatchRepositorySpy ChunkBatchRepository { get; } = new();
        public ChunkRepositorySpy ChunkRepository { get; } = new();
        public ChunkEmbeddingRepositorySpy ChunkEmbeddingRepository { get; } = new();
        public EmbeddingVectorStoreSpy EmbeddingVectorStore { get; } = new();

        public TestEmbeddingModelDefaults EmbeddingModelDefaults { get; } = new() { DefaultModel = "test-model@1" };

        public UnitOfWorkSpy UnitOfWork { get; } = new();
        public TestClockService ClockService { get; } = new(TestDates.ArtifactCreatedAtUtc);

        public RememberMemoryCommandHandler CreateHandler()
        {
            ArtifactIndexingService indexingService = new(
                ChunkBatchRepository,
                ChunkRepository,
                ChunkEmbeddingRepository,
                EmbeddingVectorStore,
                [new TestChunkingStrategy()],
                new TestBatchEmbeddingGeneratorService(),
                ClockService,
                new NoOpUsageMeterService());
            IMapper mapper = new MapperConfiguration(
                options => options.AddProfile<ApplicationMappingProfile>(),
                NullLoggerFactory.Instance).CreateMapper();

            return new RememberMemoryCommandHandler(
                WorkspaceRepository,
                ArtifactRepository,
                ArtifactRevisionRepository,
                MemoryMetadataRepository,
                BindingRepository,
                EmbeddingModelDefaults,
                indexingService,
                UnitOfWork,
                ClockService,
                mapper);
        }

        public Workspace GivenWorkspaceExists()
        {
            Workspace workspace = new WorkspaceBuilder().BuildWithoutPendingEvents();
            WorkspaceRepository.WorkspaceToReturn = workspace;
            return workspace;
        }
    }
}