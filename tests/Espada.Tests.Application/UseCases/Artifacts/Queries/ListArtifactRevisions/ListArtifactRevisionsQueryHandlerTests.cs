using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Artifacts.Queries.ListArtifactRevisions;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Tests.Application.Assertions;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.UseCases.Artifacts.Queries.ListArtifactRevisions
{
    public sealed class ListArtifactRevisionsQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WhenRevisionsExist_ShouldReturnNewestFirst()
        {
            ListArtifactRevisionsHandlerFixture fixture = new();

            (
                Artifact artifact,
                ArtifactRevision firstRevision,
                ArtifactRevision secondRevision) =
                fixture.GivenArtifactWithTwoRevisions();

            ListArtifactRevisionsQueryHandler handler =
                fixture.CreateHandler();

            ListArtifactRevisionsQuery query =
                new ListArtifactRevisionsQueryBuilder()
                    .InWorkspace(artifact.WorkspaceId.Value)
                    .ForArtifact(artifact.Id.Value)
                    .Build();

            DomainResult<ListArtifactRevisionsResponse> result =
                await handler.Handle(
                    query,
                    TestContext.Current.CancellationToken);

            ListArtifactRevisionsResponse response =
                result.ShouldSucceed();

            response.Items.Should().HaveCount(2);

            response.Items[0].Id.Should().Be(
                secondRevision.Id.Value);

            response.Items[0].Number.Should().Be(2);

            response.Items[1].Id.Should().Be(
                firstRevision.Id.Value);

            response.Items[1].Number.Should().Be(1);
        }

        [Fact]
        public async Task Handle_WhenArtifactHasNoRevisions_ShouldReturnEmptyList()
        {
            ListArtifactRevisionsHandlerFixture fixture = new();

            Artifact artifact =
                fixture.GivenArtifactWithoutRevisions();

            ListArtifactRevisionsQueryHandler handler =
                fixture.CreateHandler();

            ListArtifactRevisionsQuery query =
                new ListArtifactRevisionsQueryBuilder()
                    .InWorkspace(artifact.WorkspaceId.Value)
                    .ForArtifact(artifact.Id.Value)
                    .Build();

            DomainResult<ListArtifactRevisionsResponse> result =
                await handler.Handle(
                    query,
                    TestContext.Current.CancellationToken);

            ListArtifactRevisionsResponse response =
                result.ShouldSucceed();

            response.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WhenArtifactDoesNotExist_ShouldReturnNotFound()
        {
            ListArtifactRevisionsHandlerFixture fixture = new();

            fixture.GivenArtifactDoesNotExist();

            ListArtifactRevisionsQueryHandler handler =
                fixture.CreateHandler();

            Guid artifactId =
                ArtifactTestIds.DefaultArtifactId.Value;

            ListArtifactRevisionsQuery query =
                new ListArtifactRevisionsQueryBuilder()
                    .ForArtifact(artifactId)
                    .Build();

            DomainResult<ListArtifactRevisionsResponse> result =
                await handler.Handle(
                    query,
                    TestContext.Current.CancellationToken);

            result.ShouldFailWith(
                ArtifactApplicationErrors.NotFound(
                    artifactId));

            fixture.ArtifactRevisionRepository
                .ListByArtifactIdCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WhenArtifactBelongsToAnotherWorkspace_ShouldReturnFailure()
        {
            ListArtifactRevisionsHandlerFixture fixture = new();

            (
                Artifact artifact,
                ArtifactRevision _,
                ArtifactRevision _) =
                fixture.GivenArtifactWithTwoRevisions(
                    TestIds.AnotherWorkspaceId);

            ListArtifactRevisionsQueryHandler handler =
                fixture.CreateHandler();

            Guid requestedWorkspaceId =
                TestIds.DefaultWorkspaceId.Value;

            ListArtifactRevisionsQuery query =
                new ListArtifactRevisionsQueryBuilder()
                    .InWorkspace(requestedWorkspaceId)
                    .ForArtifact(artifact.Id.Value)
                    .Build();

            DomainResult<ListArtifactRevisionsResponse> result =
                await handler.Handle(
                    query,
                    TestContext.Current.CancellationToken);

            result.ShouldFailWith(
                ArtifactApplicationErrors.NotFoundInWorkspace(
                    artifact.Id.Value,
                    requestedWorkspaceId));

            fixture.ArtifactRevisionRepository
                .ListByArtifactIdCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_ShouldForwardCancellationToken()
        {
            ListArtifactRevisionsHandlerFixture fixture = new();

            (
                Artifact artifact,
                ArtifactRevision _,
                ArtifactRevision _) =
                fixture.GivenArtifactWithTwoRevisions();

            ListArtifactRevisionsQueryHandler handler =
                fixture.CreateHandler();

            ListArtifactRevisionsQuery query =
                new ListArtifactRevisionsQueryBuilder()
                    .InWorkspace(artifact.WorkspaceId.Value)
                    .ForArtifact(artifact.Id.Value)
                    .Build();

            using CancellationTokenSource tokenSource = new();

            CancellationToken cancellationToken =
                tokenSource.Token;

            DomainResult<ListArtifactRevisionsResponse> result =
                await handler.Handle(
                    query,
                    cancellationToken);

            result.ShouldSucceed();

            fixture.ArtifactRepository
                .GetByIdCancellationToken
                .Should()
                .Be(cancellationToken);

            fixture.ArtifactRevisionRepository
                .ListCancellationToken
                .Should()
                .Be(cancellationToken);
        }
    }
}