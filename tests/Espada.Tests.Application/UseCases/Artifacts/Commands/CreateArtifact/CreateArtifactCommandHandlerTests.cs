using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Artifacts.Commands.CreateArtifact;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Tests.Application.Assertions;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.UseCases.Artifacts.Commands.CreateArtifact
{
    public sealed class CreateArtifactCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WhenRequestIsValid_ShouldCreateArtifactAndFirstRevision()
        {
            // Arrange
            CreateArtifactHandlerFixture fixture = new();

            Workspace workspace = fixture.GivenWorkspaceExists();

            CreateArtifactCommandHandler handler = fixture.CreateHandler();

            CreateArtifactCommand command = new CreateArtifactCommandBuilder()
                .InWorkspace(workspace.Id.Value)
                .Build();

            // Act
            DomainResult<CreateArtifactResponse> result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            // Assert
            CreateArtifactResponse response = result.ShouldSucceed();

            Artifact artifact = GetAddedArtifact(fixture);
            ArtifactRevision revision = GetAddedRevision(fixture);

            response.ArtifactId.Should().Be(artifact.Id.Value);
            response.ArtifactRevisionId.Should().Be(revision.Id.Value);
            response.RevisionNumber.Should().Be(1);
            response.ContentHash.Should().Be(revision.ContentHash.Value);
            response.SizeInBytes.Should().Be(revision.SizeInBytes);
            response.CreatedAtUtc.Should().Be(TestDates.ArtifactCreatedAtUtc);

            artifact.WorkspaceId.Should().Be(workspace.Id);
            artifact.Title.Value.Should().Be(TestValues.ArtifactTitle);
            artifact.Type.Should().Be(ArtifactType.Markdown);
            artifact.Status.Should().Be(ArtifactStatusType.Active);
            artifact.CurrentRevisionId.Should().Be(revision.Id);
            artifact.CurrentRevisionNumber.Should().Be(revision.Number);
            artifact.RevisionCount.Should().Be(1);
            artifact.CreatedAtUtc.Should().Be(TestDates.ArtifactCreatedAtUtc);
            artifact.UpdatedAtUtc.Should().Be(TestDates.ArtifactCreatedAtUtc);

            revision.ArtifactId.Should().Be(artifact.Id);
            revision.Number.Value.Should().Be(1);
            revision.Content.Value.Should().Be(TestValues.ArtifactContent);
            revision.CreatedAtUtc.Should().Be(TestDates.ArtifactCreatedAtUtc);
        }

        [Fact]
        public async Task Handle_WhenRequestIsValid_ShouldPersistAndSaveOnce()
        {
            // Arrange
            CreateArtifactHandlerFixture fixture = new();

            fixture.GivenWorkspaceExists();

            CreateArtifactCommandHandler handler = fixture.CreateHandler();

            CreateArtifactCommand command = new CreateArtifactCommandBuilder().Build();

            // Act
            DomainResult<CreateArtifactResponse> result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            // Assert
            result.ShouldSucceed();

            fixture.WorkspaceRepository
                .GetByIdCallCount
                .Should()
                .Be(1);

            fixture.ArtifactRepository
                .AddCallCount
                .Should()
                .Be(1);

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
        public async Task Handle_ShouldForwardCancellationToken()
        {
            // Arrange
            CreateArtifactHandlerFixture fixture = new();

            fixture.GivenWorkspaceExists();

            CreateArtifactCommandHandler handler = fixture.CreateHandler();

            CreateArtifactCommand command = new CreateArtifactCommandBuilder().Build();

            using CancellationTokenSource tokenSource = new();

            CancellationToken cancellationToken = tokenSource.Token;

            // Act
            DomainResult<CreateArtifactResponse> result = await handler.Handle(
                command,
                cancellationToken);

            // Assert
            result.ShouldSucceed();

            fixture.WorkspaceRepository
                .GetByIdCancellationToken
                .Should()
                .Be(cancellationToken);

            fixture.ArtifactRepository
                .AddCancellationToken
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

        [Fact]
        public async Task Handle_WhenWorkspaceDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            CreateArtifactHandlerFixture fixture = new();

            fixture.GivenWorkspaceDoesNotExist();

            CreateArtifactCommandHandler handler = fixture.CreateHandler();

            Guid workspaceId = TestIds.DefaultWorkspaceId.Value;

            CreateArtifactCommand command = new CreateArtifactCommandBuilder()
                .InWorkspace(workspaceId)
                .Build();

            // Act
            DomainResult<CreateArtifactResponse> result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(WorkspaceApplicationErrors.NotFound(workspaceId));

            fixture.ArtifactRepository
                .AddCallCount
                .Should()
                .Be(0);

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
        public async Task Handle_WithEmptyWorkspaceId_ShouldNotQueryOrPersist()
        {
            // Arrange
            CreateArtifactHandlerFixture fixture = new();

            CreateArtifactCommandHandler handler = fixture.CreateHandler();

            CreateArtifactCommand command = new CreateArtifactCommandBuilder()
                .InWorkspace(Guid.Empty)
                .Build();

            // Act
            DomainResult<CreateArtifactResponse> result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(WorkspaceApplicationErrors.InvalidId);

            fixture.WorkspaceRepository
                .GetByIdCallCount
                .Should()
                .Be(0);

            fixture.ArtifactRepository
                .AddCallCount
                .Should()
                .Be(0);

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
        public async Task Handle_WithEmptyTitle_ShouldReturnDomainFailure()
        {
            // Arrange
            CreateArtifactHandlerFixture fixture = new();

            CreateArtifactCommandHandler handler = fixture.CreateHandler();

            CreateArtifactCommand command = new CreateArtifactCommandBuilder()
                .WithTitle(null)
                .Build();

            // Act
            DomainResult<CreateArtifactResponse> result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(ArtifactErrors.TitleEmpty);

            fixture.WorkspaceRepository
                .GetByIdCallCount
                .Should()
                .Be(0);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WithEmptyContent_ShouldReturnDomainFailure()
        {
            // Arrange
            CreateArtifactHandlerFixture fixture = new();

            CreateArtifactCommandHandler handler = fixture.CreateHandler();

            CreateArtifactCommand command = new CreateArtifactCommandBuilder()
                .WithContent(null)
                .Build();

            // Act
            DomainResult<CreateArtifactResponse> result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(ArtifactRevisionErrors.ContentEmpty);

            fixture.WorkspaceRepository
                .GetByIdCallCount
                .Should()
                .Be(0);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WithUnsupportedType_ShouldReturnFailure()
        {
            // Arrange
            CreateArtifactHandlerFixture fixture = new();

            CreateArtifactCommandHandler handler = fixture.CreateHandler();

            const int unsupportedTypeId = 999;

            CreateArtifactCommand command = new CreateArtifactCommandBuilder()
                .WithType(unsupportedTypeId)
                .Build();

            // Act
            DomainResult<CreateArtifactResponse> result = await handler.Handle(
                command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(ArtifactApplicationErrors.UnsupportedType(unsupportedTypeId));

            fixture.WorkspaceRepository
                .GetByIdCallCount
                .Should()
                .Be(0);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        private static Artifact GetAddedArtifact(CreateArtifactHandlerFixture fixture)
        {
            fixture.ArtifactRepository
                .AddedArtifact
                .Should()
                .NotBeNull();

            return fixture.ArtifactRepository.AddedArtifact!;
        }

        private static ArtifactRevision GetAddedRevision(CreateArtifactHandlerFixture fixture)
        {
            fixture.ArtifactRevisionRepository
                .AddedArtifactRevision
                .Should()
                .NotBeNull();

            return fixture.ArtifactRevisionRepository.AddedArtifactRevision!;
        }
    }
}