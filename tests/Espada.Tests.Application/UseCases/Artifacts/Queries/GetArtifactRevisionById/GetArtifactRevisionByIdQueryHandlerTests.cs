using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Artifacts.Queries.GetArtifactRevisionById;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Tests.Application.Assertions;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.UseCases.Artifacts.Queries.GetArtifactRevisionById
{
    public sealed class GetArtifactRevisionByIdQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WhenRevisionExists_ShouldReturnRevision()
        {
            GetArtifactRevisionByIdHandlerFixture fixture = new();

            (Artifact artifact, ArtifactRevision revision) =
                fixture.GivenRevisionExists();

            GetArtifactRevisionByIdQueryHandler handler =
                fixture.CreateHandler();

            GetArtifactRevisionByIdQuery query =
                new GetArtifactRevisionByIdQueryBuilder()
                    .InWorkspace(artifact.WorkspaceId.Value)
                    .ForArtifact(artifact.Id.Value)
                    .ForRevision(revision.Id.Value)
                    .Build();

            DomainResult<GetArtifactRevisionByIdResponse> result =
                await handler.Handle(
                    query,
                    TestContext.Current.CancellationToken);

            GetArtifactRevisionByIdResponse response =
                result.ShouldSucceed();

            response.Id.Should().Be(revision.Id.Value);
            response.ArtifactId.Should().Be(artifact.Id.Value);
            response.Number.Should().Be(1);
            response.Content.Should().Be(
                TestValues.ArtifactContent);
            response.ContentHash.Should().Be(
                revision.ContentHash.Value);
            response.SizeInBytes.Should().Be(
                revision.SizeInBytes);
            response.CreatedAtUtc.Should().Be(
                TestDates.ArtifactFirstRevisionCreatedAtUtc);
        }

        [Fact]
        public async Task Handle_WhenRevisionDoesNotExist_ShouldReturnNotFound()
        {
            GetArtifactRevisionByIdHandlerFixture fixture = new();

            Artifact artifact =
                fixture.GivenRevisionDoesNotExist();

            GetArtifactRevisionByIdQueryHandler handler =
                fixture.CreateHandler();

            Guid revisionId =
                TestIds.DefaultArtifactRevisionId.Value;

            GetArtifactRevisionByIdQuery query =
                new GetArtifactRevisionByIdQueryBuilder()
                    .InWorkspace(artifact.WorkspaceId.Value)
                    .ForArtifact(artifact.Id.Value)
                    .ForRevision(revisionId)
                    .Build();

            DomainResult<GetArtifactRevisionByIdResponse> result =
                await handler.Handle(
                    query,
                    TestContext.Current.CancellationToken);

            result.ShouldFailWith(
                ArtifactRevisionApplicationErrors.NotFound(
                    revisionId));
        }

        [Fact]
        public async Task Handle_WhenRevisionBelongsToAnotherArtifact_ShouldReturnFailure()
        {
            GetArtifactRevisionByIdHandlerFixture fixture = new();

            (
                Artifact requestedArtifact,
                ArtifactRevision foreignRevision) =
                fixture.GivenRevisionBelongsToAnotherArtifact();

            GetArtifactRevisionByIdQueryHandler handler =
                fixture.CreateHandler();

            GetArtifactRevisionByIdQuery query =
                new GetArtifactRevisionByIdQueryBuilder()
                    .InWorkspace(requestedArtifact.WorkspaceId.Value)
                    .ForArtifact(requestedArtifact.Id.Value)
                    .ForRevision(foreignRevision.Id.Value)
                    .Build();

            DomainResult<GetArtifactRevisionByIdResponse> result =
                await handler.Handle(
                    query,
                    TestContext.Current.CancellationToken);

            result.ShouldFailWith(
                ArtifactRevisionApplicationErrors.NotFoundInArtifact(
                    foreignRevision.Id.Value,
                    requestedArtifact.Id.Value));
        }

        [Fact]
        public async Task Handle_WhenArtifactDoesNotExist_ShouldReturnNotFound()
        {
            GetArtifactRevisionByIdHandlerFixture fixture = new();

            fixture.GivenArtifactDoesNotExist();

            GetArtifactRevisionByIdQueryHandler handler =
                fixture.CreateHandler();

            Guid artifactId =
                TestIds.DefaultArtifactId.Value;

            GetArtifactRevisionByIdQuery query =
                new GetArtifactRevisionByIdQueryBuilder()
                    .ForArtifact(artifactId)
                    .Build();

            DomainResult<GetArtifactRevisionByIdResponse> result =
                await handler.Handle(
                    query,
                    TestContext.Current.CancellationToken);

            result.ShouldFailWith(
                ArtifactApplicationErrors.NotFound(
                    artifactId));

            fixture.ArtifactRevisionRepository
                .GetByIdCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_ShouldForwardCancellationToken()
        {
            GetArtifactRevisionByIdHandlerFixture fixture = new();

            (Artifact artifact, ArtifactRevision revision) =
                fixture.GivenRevisionExists();

            GetArtifactRevisionByIdQueryHandler handler =
                fixture.CreateHandler();

            GetArtifactRevisionByIdQuery query =
                new GetArtifactRevisionByIdQueryBuilder()
                    .InWorkspace(artifact.WorkspaceId.Value)
                    .ForArtifact(artifact.Id.Value)
                    .ForRevision(revision.Id.Value)
                    .Build();

            using CancellationTokenSource tokenSource = new();

            CancellationToken cancellationToken =
                tokenSource.Token;

            DomainResult<GetArtifactRevisionByIdResponse> result =
                await handler.Handle(
                    query,
                    cancellationToken);

            result.ShouldSucceed();

            fixture.ArtifactRepository
                .GetByIdCancellationToken
                .Should()
                .Be(cancellationToken);

            fixture.ArtifactRevisionRepository
                .GetByIdCancellationToken
                .Should()
                .Be(cancellationToken);
        }
    }
}