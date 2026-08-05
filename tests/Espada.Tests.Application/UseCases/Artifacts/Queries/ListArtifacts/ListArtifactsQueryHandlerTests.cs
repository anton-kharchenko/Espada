using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Artifacts.Queries.ListArtifacts;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.UseCases.Artifacts.Queries.ListArtifacts
{
    public sealed class ListArtifactsQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WhenArtifactsExist_ShouldReturnNewestFirst()
        {
            ListArtifactsHandlerFixture fixture = new();

            Workspace workspace = fixture.GivenWorkspaceExists();

            (Artifact first, Artifact second) = fixture.GivenArtifactsExist();

            ListArtifactsQueryHandler handler = fixture.CreateHandler();

            ListArtifactsQuery query = new ListArtifactsQueryBuilder()
                .InWorkspace(workspace.Id.Value)
                .Build();

            DomainResult<ListArtifactsResponse> result =
                await handler.Handle(query, TestContext.Current.CancellationToken);

            ListArtifactsResponse response = result.ShouldSucceed();

            response.Items.Should().HaveCount(2);

            response.Items[0].Id.Should().Be(second.Id.Value);

            response.Items[1].Id.Should().Be(first.Id.Value);

            fixture.ArtifactRepository
                .ReceivedWorkspaceId
                .Should()
                .Be(workspace.Id);
        }

        [Fact]
        public async Task Handle_WhenNoArtifactsExist_ShouldReturnEmptyList()
        {
            ListArtifactsHandlerFixture fixture = new();

            Workspace workspace = fixture.GivenWorkspaceExists();

            fixture.GivenNoArtifactsExist();

            ListArtifactsQueryHandler handler =
                fixture.CreateHandler();

            ListArtifactsQuery query =
                new ListArtifactsQueryBuilder()
                    .InWorkspace(workspace.Id.Value)
                    .Build();

            DomainResult<ListArtifactsResponse> result =
                await handler.Handle(
                    query,
                    TestContext.Current.CancellationToken);

            ListArtifactsResponse response =
                result.ShouldSucceed();

            response.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WhenWorkspaceDoesNotExist_ShouldReturnNotFound()
        {
            ListArtifactsHandlerFixture fixture = new();

            fixture.GivenWorkspaceDoesNotExist();

            ListArtifactsQueryHandler handler =
                fixture.CreateHandler();

            Guid workspaceId =
                TestIds.DefaultWorkspaceId.Value;

            ListArtifactsQuery query =
                new ListArtifactsQueryBuilder()
                    .InWorkspace(workspaceId)
                    .Build();

            DomainResult<ListArtifactsResponse> result =
                await handler.Handle(
                    query,
                    TestContext.Current.CancellationToken);

            result.ShouldFailWith(
                WorkspaceApplicationErrors.NotFound(workspaceId));

            fixture.ArtifactRepository
                .ListByWorkspaceIdCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_ShouldForwardCancellationToken()
        {
            ListArtifactsHandlerFixture fixture = new();

            Workspace workspace = fixture.GivenWorkspaceExists();

            fixture.GivenNoArtifactsExist();

            ListArtifactsQueryHandler handler =
                fixture.CreateHandler();

            ListArtifactsQuery query =
                new ListArtifactsQueryBuilder()
                    .InWorkspace(workspace.Id.Value)
                    .Build();

            using CancellationTokenSource source = new();

            CancellationToken cancellationToken = source.Token;

            DomainResult<ListArtifactsResponse> result =
                await handler.Handle(
                    query,
                    cancellationToken);

            result.ShouldSucceed();

            fixture.WorkspaceRepository
                .GetByIdCancellationToken
                .Should()
                .Be(cancellationToken);

            fixture.ArtifactRepository
                .ListCancellationToken
                .Should()
                .Be(cancellationToken);
        }
    }
}