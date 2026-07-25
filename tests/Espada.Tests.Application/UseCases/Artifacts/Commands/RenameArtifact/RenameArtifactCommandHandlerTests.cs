using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Artifacts.Commands.RenameArtifact;
using Espada.Domain.Aggregates;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Tests.Application.Assertions;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.UseCases.Artifacts.Commands.RenameArtifact
{
    public sealed class RenameArtifactCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WhenArtifactExists_ShouldRenameArtifact()
        {
            RenameArtifactHandlerFixture fixture = new();

            Artifact artifact = fixture.GivenArtifactExists();

            RenameArtifactCommandHandler handler =
                fixture.CreateHandler();

            RenameArtifactCommand command =
                new RenameArtifactCommandBuilder()
                    .InWorkspace(artifact.WorkspaceId.Value)
                    .ForArtifact(artifact.Id.Value)
                    .Build();

            DomainResult result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            result.ShouldSucceed();

            artifact.Title.Value.Should().Be(
                ArtifactTestValues.RenamedTitle);

            artifact.UpdatedAtUtc.Should().Be(
                ArtifactTestDates.RenamedAtUtc);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(1);
        }

        [Fact]
        public async Task Handle_WhenArtifactIsArchived_ShouldReturnFailure()
        {
            RenameArtifactHandlerFixture fixture = new();

            Artifact artifact =
                fixture.GivenArchivedArtifactExists();

            RenameArtifactCommandHandler handler =
                fixture.CreateHandler();

            RenameArtifactCommand command =
                new RenameArtifactCommandBuilder()
                    .InWorkspace(artifact.WorkspaceId.Value)
                    .ForArtifact(artifact.Id.Value)
                    .Build();

            DomainResult result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            result.ShouldFailWith(
                ArtifactErrors.ArchivedArtifactCannotBeRenamed);

            artifact.Title.Value.Should().Be(
                ArtifactTestValues.Title);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WhenArtifactDoesNotExist_ShouldReturnNotFound()
        {
            RenameArtifactHandlerFixture fixture = new();

            fixture.GivenArtifactDoesNotExist();

            RenameArtifactCommandHandler handler =
                fixture.CreateHandler();

            Guid artifactId =
                ArtifactTestIds.DefaultArtifactId.Value;

            RenameArtifactCommand command =
                new RenameArtifactCommandBuilder()
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
        public async Task Handle_WithInvalidTitle_ShouldNotQueryRepository()
        {
            RenameArtifactHandlerFixture fixture = new();

            RenameArtifactCommandHandler handler =
                fixture.CreateHandler();

            RenameArtifactCommand command =
                new RenameArtifactCommandBuilder()
                    .WithTitle(null)
                    .Build();

            DomainResult result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            result.ShouldFailWith(ArtifactErrors.TitleEmpty);

            fixture.ArtifactRepository
                .GetByIdCallCount
                .Should()
                .Be(0);
        }
    }
}