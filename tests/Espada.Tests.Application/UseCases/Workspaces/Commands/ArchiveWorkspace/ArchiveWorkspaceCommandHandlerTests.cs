using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Workspaces.Commands.ArchiveWorkspace;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;

namespace Espada.Tests.Application.UseCases.Workspaces.Commands.ArchiveWorkspace
{
    public sealed class ArchiveWorkspaceCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WhenWorkspaceIsActive_ShouldArchiveWorkspace()
        {
            // Arrange
            ArchiveWorkspaceHandlerFixture fixture = new();

            Workspace workspace = fixture.GivenActiveWorkspaceExists();

            ArchiveWorkspaceCommandHandler handler = fixture.CreateHandler();

            ArchiveWorkspaceCommand command = new(workspace.Id.Value);

            // Act
            DomainResult result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldSucceed();

            workspace.Status.Should().Be(WorkspaceStatusType.Archived);

            workspace.ArchivedAtUtc.Should().Be(TestDates.WorkspaceArchivedAtUtc);
        }

        [Fact]
        public async Task Handle_WhenWorkspaceIsActive_ShouldUseClockTime()
        {
            // Arrange
            ArchiveWorkspaceHandlerFixture fixture = new() { ClockService = { UtcNow = TestDates.LaterUtc } };

            Workspace workspace = fixture.GivenActiveWorkspaceExists();

            ArchiveWorkspaceCommandHandler handler = fixture.CreateHandler();

            ArchiveWorkspaceCommand command = new(workspace.Id.Value);

            // Act
            DomainResult result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldSucceed();

            workspace.ArchivedAtUtc.Should().Be(TestDates.LaterUtc);
        }

        [Fact]
        public async Task Handle_WhenWorkspaceIsActive_ShouldQueryRepositoryOnce()
        {
            // Arrange
            ArchiveWorkspaceHandlerFixture fixture = new();

            Workspace workspace = fixture.GivenActiveWorkspaceExists();

            ArchiveWorkspaceCommandHandler handler = fixture.CreateHandler();

            ArchiveWorkspaceCommand command = new(workspace.Id.Value);

            // Act
            DomainResult result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldSucceed();

            fixture.WorkspaceRepository
                .GetByIdCallCount
                .Should()
                .Be(1);

            fixture.WorkspaceRepository
                .ReceivedWorkspaceId
                .Should()
                .Be(workspace.Id);
        }

        [Fact]
        public async Task Handle_WhenWorkspaceIsActive_ShouldSaveChangesOnce()
        {
            // Arrange
            ArchiveWorkspaceHandlerFixture fixture = new();

            Workspace workspace = fixture.GivenActiveWorkspaceExists();

            ArchiveWorkspaceCommandHandler handler = fixture.CreateHandler();

            ArchiveWorkspaceCommand command = new(workspace.Id.Value);

            // Act
            DomainResult result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldSucceed();

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(1);
        }

        [Fact]
        public async Task Handle_ShouldForwardCancellationToken()
        {
            // Arrange
            ArchiveWorkspaceHandlerFixture fixture = new();

            Workspace workspace = fixture.GivenActiveWorkspaceExists();

            ArchiveWorkspaceCommandHandler handler = fixture.CreateHandler();

            ArchiveWorkspaceCommand command = new(workspace.Id.Value);

            using CancellationTokenSource source = new();

            CancellationToken cancellationToken = source.Token;

            // Act
            DomainResult result = await handler.Handle(command, cancellationToken);

            // Assert
            result.ShouldSucceed();

            fixture.WorkspaceRepository
                .GetByIdCancellationToken
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
            ArchiveWorkspaceHandlerFixture fixture = new();

            fixture.GivenWorkspaceDoesNotExist();

            ArchiveWorkspaceCommandHandler handler = fixture.CreateHandler();

            Guid workspaceId = TestIds.DefaultWorkspaceId.Value;

            ArchiveWorkspaceCommand command = new(workspaceId);

            // Act
            DomainResult result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldFailWith(WorkspaceApplicationErrors.NotFound(workspaceId));

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WhenWorkspaceIsAlreadyArchived_ShouldReturnFailure()
        {
            // Arrange
            ArchiveWorkspaceHandlerFixture fixture = new();

            Workspace workspace = fixture.GivenArchivedWorkspaceExists();

            DateTimeOffset? originalArchivedAtUtc = workspace.ArchivedAtUtc;

            ArchiveWorkspaceCommandHandler handler = fixture.CreateHandler();

            ArchiveWorkspaceCommand command = new(workspace.Id.Value);

            // Act
            DomainResult result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldFailWith(WorkspaceErrors.AlreadyArchived);

            workspace.Status.Should().Be(WorkspaceStatusType.Archived);

            workspace.ArchivedAtUtc.Should().Be(originalArchivedAtUtc);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WithEmptyWorkspaceId_ShouldReturnInvalidId()
        {
            // Arrange
            ArchiveWorkspaceHandlerFixture fixture = new();

            ArchiveWorkspaceCommandHandler handler = fixture.CreateHandler();

            ArchiveWorkspaceCommand command = new(Guid.Empty);

            // Act
            DomainResult result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShouldFailWith(WorkspaceApplicationErrors.InvalidId);

            fixture.WorkspaceRepository
                .GetByIdCallCount
                .Should()
                .Be(0);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }
    }
}