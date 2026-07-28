using AutoMapper;
using Espada.Application.Mappings;
using Espada.Application.UseCases.Memories.Queries.SearchMemory;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;
using Microsoft.Extensions.Logging.Abstractions;

namespace Espada.Tests.Application.UseCases.Memories.Queries.SearchMemory
{
    public sealed class SearchMemoryQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WithoutDefaultModel_ShouldUseFtsOnlyMemorySearch()
        {
            SearchFixture fixture = new(null);

            DomainResult<SearchMemoryResponse> result = await fixture.Handler.Handle(
                fixture.CreateQuery(),
                TestContext.Current.CancellationToken);

            result.ShouldSucceed();
            fixture.EmbeddingGenerator.GenerateCallCount.Should().Be(0);
            fixture.SearchStore.ReceivedSearch!.QueryVector.Should().BeEmpty();
            fixture.SearchStore.ReceivedSearch.ArtifactKinds.Should().Equal(
                ArtifactKindType.Memory.Name);
        }

        [Fact]
        public async Task Handle_WithDefaultModel_ShouldUseHybridMemorySearch()
        {
            SearchFixture fixture = new("test-model@1");

            DomainResult<SearchMemoryResponse> result = await fixture.Handler.Handle(
                fixture.CreateQuery(),
                TestContext.Current.CancellationToken);

            result.ShouldSucceed();
            fixture.EmbeddingGenerator.GenerateCallCount.Should().Be(1);
            fixture.EmbeddingGenerator.ReceivedModelIdentifier.Should().Be("test-model");
            fixture.EmbeddingGenerator.ReceivedModelVersion.Should().Be("1");
            fixture.SearchStore.ReceivedSearch!.QueryVector.Should().Equal(1f, 0f, 0f);
        }

        private sealed class SearchFixture
        {
            public SearchFixture(string? defaultModel)
            {
                Workspace workspace = new WorkspaceBuilder().BuildWithoutPendingEvents();
                WorkspaceRepository.WorkspaceToReturn = workspace;
                WorkspaceId = workspace.Id.Value;
                EmbeddingModelDefaults.DefaultModel = defaultModel;
                IMapper mapper = new MapperConfiguration(
                    options => options.AddProfile<ApplicationMappingProfile>(),
                    NullLoggerFactory.Instance).CreateMapper();
                Handler = new SearchMemoryQueryHandler(
                    WorkspaceRepository,
                    new MemorySearchStoreSpy(),
                    SearchStore,
                    EmbeddingModelDefaults,
                    EmbeddingGenerator,
                    new TestClockService(TestDates.ArtifactCreatedAtUtc),
                    mapper);
            }

            public Guid WorkspaceId { get; }
            public SearchMemoryQueryHandler Handler { get; }
            public WorkspaceRepositorySpy WorkspaceRepository { get; } = new();
            public WorkspaceContextSearchStoreSpy SearchStore { get; } = new();
            public TestEmbeddingModelDefaults EmbeddingModelDefaults { get; } = new();
            public EmbeddingGeneratorServiceSpy EmbeddingGenerator { get; } = new();

            public SearchMemoryQuery CreateQuery()
            {
                return new SearchMemoryQuery(
                    WorkspaceId,
                    "PostgreSQL",
                    [MemoryCategoryType.Decision.Id]);
            }
        }
    }
}