using Espada.Application.ApplicationErrors;
using Espada.Application.Models;
using Espada.Application.UseCases.Search.Queries.UnifiedSearch;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Tests.Application.Fakes;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.UseCases.Search.Queries.UnifiedSearch
{
    public sealed class UnifiedSearchQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WithoutDefaultModel_ShouldUseFtsOnly()
        {
            SearchFixture fixture = new(null);

            DomainResult<UnifiedSearchResponse> result = await fixture.Handler.Handle(
                fixture.CreateQuery(), TestContext.Current.CancellationToken);

            result.ShouldSucceed();
            fixture.EmbeddingGenerator.GenerateCallCount.Should().Be(0);
            fixture.SearchStore.ReceivedSearch!.QueryVector.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithDefaultModel_ShouldGenerateOneQueryEmbedding()
        {
            SearchFixture fixture = new("test-model@1");

            DomainResult<UnifiedSearchResponse> result = await fixture.Handler.Handle(
                fixture.CreateQuery(), TestContext.Current.CancellationToken);

            result.ShouldSucceed();
            fixture.EmbeddingGenerator.GenerateCallCount.Should().Be(1);
            fixture.EmbeddingGenerator.ReceivedModelIdentifier.Should().Be("test-model");
            fixture.EmbeddingGenerator.ReceivedModelVersion.Should().Be("1");
            fixture.SearchStore.ReceivedSearch!.QueryVector.Should().Equal(1f, 0f, 0f);
        }

        [Fact]
        public async Task Handle_WithInvalidDefaultModel_ShouldReturnFailure()
        {
            SearchFixture fixture = new("invalid");

            DomainResult<UnifiedSearchResponse> result = await fixture.Handler.Handle(
                fixture.CreateQuery(), TestContext.Current.CancellationToken);

            result.ShouldFailWith(UnifiedSearchApplicationErrors.InvalidEmbeddingModel);
            fixture.EmbeddingGenerator.GenerateCallCount.Should().Be(0);
        }

        private sealed class SearchFixture
        {
            public SearchFixture(string? defaultModel)
            {
                Workspace workspace = new WorkspaceBuilder().BuildWithoutPendingEvents();
                WorkspaceRepository.WorkspaceToReturn = workspace;
                WorkspaceId = workspace.Id.Value;
                EmbeddingModelDefaults.DefaultModel = defaultModel;
                Handler = new UnifiedSearchQueryHandler(WorkspaceRepository, SearchStore, MetadataStore,
                    EmbeddingModelDefaults, EmbeddingGenerator, new TestClockService(TestDates.ArtifactCreatedAtUtc));
            }

            public Guid WorkspaceId { get; }
            public UnifiedSearchQueryHandler Handler { get; }
            public WorkspaceRepositorySpy WorkspaceRepository { get; } = new();
            public WorkspaceContextSearchStoreSpy SearchStore { get; } = new();
            public UnifiedSearchMetadataStoreSpy MetadataStore { get; } = new();
            public TestEmbeddingModelDefaults EmbeddingModelDefaults { get; } = new();
            public EmbeddingGeneratorServiceSpy EmbeddingGenerator { get; } = new();

            public UnifiedSearchQuery CreateQuery()
            {
                return new UnifiedSearchQuery(WorkspaceId, "PostgreSQL", 20);
            }
        }
    }
}