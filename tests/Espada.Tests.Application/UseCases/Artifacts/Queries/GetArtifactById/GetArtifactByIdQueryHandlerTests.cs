using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Artifacts.Queries.GetArtifactById;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Tests.Application.Assertions;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.UseCases.Artifacts.Queries.GetArtifactById
{
    public sealed class GetArtifactByIdQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WhenArtifactExists_ShouldReturnArtifact()
        {
            GetArtifactByIdHandlerFixture fixture = new();

            Artifact artifact = fixture.GivenArtifactExists();

            GetArtifactByIdQueryHandler handler =
                fixture.CreateHandler();

            GetArtifactByIdQuery query =
                new GetArtifactByIdQueryBuilder()
                    .InWorkspace(artifact.WorkspaceId.Value)
                    .ForArtifact(artifact.Id.Value)
                    .Build();

            DomainResult<GetArtifactByIdResponse> result =
                await handler.Handle(
                    query,
                    TestContext.Current.CancellationToken);

            GetArtifactByIdResponse response =
                result.ShouldSucceed();

            response.Id.Should().Be(artifact.Id.Value);
            response.WorkspaceId.Should().Be(artifact.WorkspaceId.Value);
            response.Title.Should().Be(TestValues.ArtifactTitle);
            response.TypeId.Should().Be(ArtifactType.Markdown.Id);
            response.TypeName.Should().Be(ArtifactType.Markdown.Name);
            response.StatusId.Should().Be(ArtifactStatusType.Active.Id);
            response.CurrentRevisionId.Should().Be(
                TestIds.DefaultArtifactRevisionId.Value);
            response.CurrentRevisionNumber.Should().Be(1);
            response.RevisionCount.Should().Be(1);
            response.ArchivedAtUtc.Should().BeNull();
        }

        [Fact]
        public async Task Handle_WhenArtifactDoesNotExist_ShouldReturnNotFound()
        {
            GetArtifactByIdHandlerFixture fixture = new();

            fixture.GivenArtifactDoesNotExist();

            GetArtifactByIdQueryHandler handler =
                fixture.CreateHandler();

            Guid artifactId =
                TestIds.DefaultArtifactId.Value;

            GetArtifactByIdQuery query =
                new GetArtifactByIdQueryBuilder()
                    .ForArtifact(artifactId)
                    .Build();

            DomainResult<GetArtifactByIdResponse> result =
                await handler.Handle(
                    query,
                    TestContext.Current.CancellationToken);

            result.ShouldFailWith(
                ArtifactApplicationErrors.NotFound(artifactId));
        }

        [Fact]
        public async Task Handle_WhenArtifactBelongsToAnotherWorkspace_ShouldReturnFailure()
        {
            GetArtifactByIdHandlerFixture fixture = new();

            Artifact artifact =
                fixture.GivenArtifactExists(
                    TestIds.AnotherWorkspaceId);

            GetArtifactByIdQueryHandler handler =
                fixture.CreateHandler();

            Guid workspaceId =
                TestIds.DefaultWorkspaceId.Value;

            GetArtifactByIdQuery query =
                new GetArtifactByIdQueryBuilder()
                    .InWorkspace(workspaceId)
                    .ForArtifact(artifact.Id.Value)
                    .Build();

            DomainResult<GetArtifactByIdResponse> result =
                await handler.Handle(
                    query,
                    TestContext.Current.CancellationToken);

            result.ShouldFailWith(
                ArtifactApplicationErrors.NotFoundInWorkspace(
                    artifact.Id.Value,
                    workspaceId));
        }

        [Fact]
        public async Task Handle_ShouldForwardCancellationToken()
        {
            GetArtifactByIdHandlerFixture fixture = new();

            Artifact artifact = fixture.GivenArtifactExists();

            GetArtifactByIdQueryHandler handler =
                fixture.CreateHandler();

            GetArtifactByIdQuery query =
                new GetArtifactByIdQueryBuilder()
                    .InWorkspace(artifact.WorkspaceId.Value)
                    .ForArtifact(artifact.Id.Value)
                    .Build();

            using CancellationTokenSource source = new();

            CancellationToken cancellationToken = source.Token;

            DomainResult<GetArtifactByIdResponse> result =
                await handler.Handle(query, cancellationToken);

            result.ShouldSucceed();

            fixture.ArtifactRepository
                .GetByIdCancellationToken
                .Should()
                .Be(cancellationToken);
        }
    }
}