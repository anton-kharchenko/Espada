using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Artifacts.Commands.ArchiveArtifact;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Tests.Application.Assertions;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.UseCases.Artifacts.Commands.ArchiveArtifact
{
    public sealed class ArchiveArtifactCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WhenArtifactIsActive_ShouldArchiveArtifact()
        {
            ArchiveArtifactHandlerFixture fixture = new();

            Artifact artifact = fixture.GivenArtifactExists();

            ArchiveArtifactCommandHandler handler =
                fixture.CreateHandler();

            ArchiveArtifactCommand command =
                new ArchiveArtifactCommandBuilder()
                    .InWorkspace(artifact.WorkspaceId.Value)
                    .ForArtifact(artifact.Id.Value)
                    .Build();

            DomainResult result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            result.ShouldSucceed();

            artifact.Status.Should().Be(
                ArtifactStatusType.Archived);

            artifact.ArchivedAtUtc.Should().Be(
                TestDates.ArtifactArchivedAtUtc);

            artifact.UpdatedAtUtc.Should().Be(
                TestDates.ArtifactArchivedAtUtc);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(1);
        }

        [Fact]
        public async Task Handle_WhenArtifactIsAlreadyArchived_ShouldReturnFailure()
        {
            ArchiveArtifactHandlerFixture fixture = new();

            Artifact artifact =
                fixture.GivenArchivedArtifactExists();

            ArchiveArtifactCommandHandler handler =
                fixture.CreateHandler();

            ArchiveArtifactCommand command =
                new ArchiveArtifactCommandBuilder()
                    .InWorkspace(artifact.WorkspaceId.Value)
                    .ForArtifact(artifact.Id.Value)
                    .Build();

            DomainResult result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            result.ShouldFailWith(
                ArtifactErrors.AlreadyArchived);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WhenArtifactDoesNotExist_ShouldReturnNotFound()
        {
            ArchiveArtifactHandlerFixture fixture = new();

            fixture.GivenArtifactDoesNotExist();

            ArchiveArtifactCommandHandler handler =
                fixture.CreateHandler();

            Guid artifactId =
                TestIds.DefaultArtifactId.Value;

            ArchiveArtifactCommand command =
                new ArchiveArtifactCommandBuilder()
                    .ForArtifact(artifactId)
                    .Build();

            DomainResult result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            result.ShouldFailWith(
                ArtifactApplicationErrors.NotFound(artifactId));

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WhenArtifactBelongsToAnotherWorkspace_ShouldReturnFailure()
        {
            ArchiveArtifactHandlerFixture fixture = new();

            Artifact artifact =
                fixture.GivenArtifactExists(
                    TestIds.AnotherWorkspaceId);

            ArchiveArtifactCommandHandler handler =
                fixture.CreateHandler();

            Guid workspaceId =
                TestIds.DefaultWorkspaceId.Value;

            ArchiveArtifactCommand command =
                new ArchiveArtifactCommandBuilder()
                    .InWorkspace(workspaceId)
                    .ForArtifact(artifact.Id.Value)
                    .Build();

            DomainResult result = await handler.Handle(
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
    }
}