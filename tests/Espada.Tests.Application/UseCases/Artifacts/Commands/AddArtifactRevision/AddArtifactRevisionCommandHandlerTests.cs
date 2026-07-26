using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Artifacts.Commands.AddArtifactRevision;
using Espada.Domain.Aggregates;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.UseCases.Artifacts.Commands.AddArtifactRevision
{
    public sealed class AddArtifactRevisionCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WhenArtifactExists_ShouldCreateNextRevision()
        {
            AddArtifactRevisionHandlerFixture fixture = new();

            Artifact artifact =
                fixture.GivenArtifactExists();

            AddArtifactRevisionCommandHandler handler =
                fixture.CreateHandler();

            AddArtifactRevisionCommand command =
                new AddArtifactRevisionCommandBuilder()
                    .InWorkspace(artifact.WorkspaceId.Value)
                    .ForArtifact(artifact.Id.Value)
                    .Build();

            DomainResult<AddArtifactRevisionResponse> result =
                await handler.Handle(
                    command,
                    TestContext.Current.CancellationToken);

            AddArtifactRevisionResponse response =
                result.ShouldSucceed();

            fixture.ArtifactRevisionRepository
                .AddedArtifactRevision
                .Should()
                .NotBeNull();

            ArtifactRevision revision =
                fixture.ArtifactRevisionRepository.AddedArtifactRevision!;

            response.ArtifactId.Should().Be(artifact.Id.Value);
            response.ArtifactRevisionId.Should().Be(revision.Id.Value);
            response.RevisionNumber.Should().Be(2);
            response.ContentHash.Should().Be(revision.ContentHash.Value);
            response.SizeInBytes.Should().Be(revision.SizeInBytes);
            response.CreatedAtUtc.Should().Be(
                TestDates.ArtifactSecondRevisionCreatedAtUtc);

            artifact.CurrentRevisionId.Should().Be(revision.Id);
            artifact.CurrentRevisionNumber.Should().Be(revision.Number);
            artifact.RevisionCount.Should().Be(2);
            artifact.UpdatedAtUtc.Should().Be(
                TestDates.ArtifactSecondRevisionCreatedAtUtc);

            revision.ArtifactId.Should().Be(artifact.Id);
            revision.Number.Value.Should().Be(2);
            revision.Content.Value.Should().Be(
                TestValues.AnotherArtifactContent);
        }

        [Fact]
        public async Task Handle_WhenArtifactExists_ShouldPersistAndSaveOnce()
        {
            AddArtifactRevisionHandlerFixture fixture = new();

            Artifact artifact =
                fixture.GivenArtifactExists();

            AddArtifactRevisionCommandHandler handler =
                fixture.CreateHandler();

            AddArtifactRevisionCommand command =
                new AddArtifactRevisionCommandBuilder()
                    .InWorkspace(artifact.WorkspaceId.Value)
                    .ForArtifact(artifact.Id.Value)
                    .Build();

            DomainResult<AddArtifactRevisionResponse> result =
                await handler.Handle(
                    command,
                    TestContext.Current.CancellationToken);

            result.ShouldSucceed();

            fixture.ArtifactRepository
                .GetByIdCallCount
                .Should()
                .Be(1);

            fixture.ArtifactRepository
                .AddCallCount
                .Should()
                .Be(0);

            fixture.ArtifactRevisionRepository
                .AddCallCount
                .Should()
                .Be(1);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(1);
        }

        [Fact]
        public async Task Handle_WhenArtifactIsArchived_ShouldReturnFailure()
        {
            AddArtifactRevisionHandlerFixture fixture = new();

            Artifact artifact =
                fixture.GivenArchivedArtifactExists();

            AddArtifactRevisionCommandHandler handler =
                fixture.CreateHandler();

            AddArtifactRevisionCommand command =
                new AddArtifactRevisionCommandBuilder()
                    .InWorkspace(artifact.WorkspaceId.Value)
                    .ForArtifact(artifact.Id.Value)
                    .Build();

            DomainResult<AddArtifactRevisionResponse> result =
                await handler.Handle(
                    command,
                    TestContext.Current.CancellationToken);

            result.ShouldFailWith(
                ArtifactRevisionErrors.ArtifactArchived);

            fixture.ArtifactRevisionRepository
                .AddCallCount
                .Should()
                .Be(0);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WhenArtifactDoesNotExist_ShouldReturnNotFound()
        {
            AddArtifactRevisionHandlerFixture fixture = new();

            fixture.GivenArtifactDoesNotExist();

            AddArtifactRevisionCommandHandler handler =
                fixture.CreateHandler();

            Guid artifactId =
                TestIds.DefaultArtifactId.Value;

            AddArtifactRevisionCommand command =
                new AddArtifactRevisionCommandBuilder()
                    .ForArtifact(artifactId)
                    .Build();

            DomainResult<AddArtifactRevisionResponse> result =
                await handler.Handle(
                    command,
                    TestContext.Current.CancellationToken);

            result.ShouldFailWith(
                ArtifactApplicationErrors.NotFound(artifactId));

            fixture.ArtifactRevisionRepository
                .AddCallCount
                .Should()
                .Be(0);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WhenArtifactBelongsToAnotherWorkspace_ShouldReturnFailure()
        {
            AddArtifactRevisionHandlerFixture fixture = new();

            Artifact artifact =
                fixture.GivenArtifactExists(
                    TestIds.AnotherWorkspaceId);

            AddArtifactRevisionCommandHandler handler =
                fixture.CreateHandler();

            Guid workspaceId =
                TestIds.DefaultWorkspaceId.Value;

            AddArtifactRevisionCommand command =
                new AddArtifactRevisionCommandBuilder()
                    .InWorkspace(workspaceId)
                    .ForArtifact(artifact.Id.Value)
                    .Build();

            DomainResult<AddArtifactRevisionResponse> result =
                await handler.Handle(
                    command,
                    TestContext.Current.CancellationToken);

            result.ShouldFailWith(
                ArtifactApplicationErrors.NotFoundInWorkspace(
                    artifact.Id.Value,
                    workspaceId));

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_ShouldForwardCancellationToken()
        {
            AddArtifactRevisionHandlerFixture fixture = new();

            Artifact artifact =
                fixture.GivenArtifactExists();

            AddArtifactRevisionCommandHandler handler =
                fixture.CreateHandler();

            AddArtifactRevisionCommand command =
                new AddArtifactRevisionCommandBuilder()
                    .InWorkspace(artifact.WorkspaceId.Value)
                    .ForArtifact(artifact.Id.Value)
                    .Build();

            using CancellationTokenSource tokenSource = new();

            CancellationToken cancellationToken =
                tokenSource.Token;

            DomainResult<AddArtifactRevisionResponse> result =
                await handler.Handle(
                    command,
                    cancellationToken);

            result.ShouldSucceed();

            fixture.ArtifactRepository
                .GetByIdCancellationToken
                .Should()
                .Be(cancellationToken);

            fixture.ArtifactRevisionRepository
                .AddCancellationToken
                .Should()
                .Be(cancellationToken);

            fixture.UnitOfWork
                .ReceivedCancellationToken
                .Should()
                .Be(cancellationToken);
        }
    }
}