using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Sources.Commands.ArchiveSource;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Tests.Application.Assertions;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;

namespace Espada.Tests.Application.UseCases.Sources.Commands.ArchiveSource
{
    public sealed class ArchiveSourceCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WhenSourceIsActive_ShouldArchiveSource()
        {
            // Arrange
            ArchiveSourceHandlerFixture fixture = new();

            Source source = fixture.GivenActiveSourceExists();

            ArchiveSourceCommandHandler handler = fixture.CreateHandler();

            ArchiveSourceCommand command = new(source.WorkspaceId.Value, source.Id.Value);

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldSucceed();

            source.Status.Should().Be(SourceStatusType.Archived);
        }

        [Fact]
        public async Task Handle_WhenSourceIsActive_ShouldQueryRepositoryOnce()
        {
            // Arrange
            ArchiveSourceHandlerFixture fixture = new();

            Source source = fixture.GivenActiveSourceExists();

            ArchiveSourceCommandHandler handler = fixture.CreateHandler();

            ArchiveSourceCommand command = new(source.WorkspaceId.Value, source.Id.Value);

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldSucceed();

            fixture.SourceRepository
                .GetByIdCallCount
                .Should()
                .Be(1);

            fixture.SourceRepository
                .ReceivedSourceId
                .Should()
                .Be(source.Id);
        }

        [Fact]
        public async Task Handle_WhenSourceIsActive_ShouldSaveChangesOnce()
        {
            // Arrange
            ArchiveSourceHandlerFixture fixture = new();

            Source source = fixture.GivenActiveSourceExists();

            ArchiveSourceCommandHandler handler = fixture.CreateHandler();

            ArchiveSourceCommand command = new(source.WorkspaceId.Value, source.Id.Value);

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

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
            ArchiveSourceHandlerFixture fixture = new();

            Source source = fixture.GivenActiveSourceExists();

            ArchiveSourceCommandHandler handler = fixture.CreateHandler();

            ArchiveSourceCommand command = new(
                source.WorkspaceId.Value, source.Id.Value);

            using CancellationTokenSource cancellationTokenSource = new();

            CancellationToken cancellationToken = cancellationTokenSource.Token;

            // Act
            DomainResult result = await handler.Handle(command, cancellationToken);

            // Assert
            result.ShouldSucceed();

            fixture.SourceRepository
                .GetByIdCancellationToken
                .Should()
                .Be(cancellationToken);

            fixture.UnitOfWork
                .ReceivedCancellationToken
                .Should()
                .Be(cancellationToken);
        }

        [Fact]
        public async Task Handle_WhenSourceDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            ArchiveSourceHandlerFixture fixture = new();

            fixture.GivenSourceDoesNotExist();

            ArchiveSourceCommandHandler handler = fixture.CreateHandler();

            Guid sourceId = TestIds.SourceId.Value;

            ArchiveSourceCommand command = new(TestIds.WorkspaceId.Value, sourceId);

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(SourceApplicationErrors.NotFound(sourceId));

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WhenSourceBelongsToAnotherWorkspace_ShouldReturnNotFoundInWorkspace()
        {
            // Arrange
            ArchiveSourceHandlerFixture fixture =
                new();

            Source source = fixture.GivenActiveSourceExists(TestIds.AnotherWorkspaceId);

            ArchiveSourceCommandHandler handler = fixture.CreateHandler();

            Guid requestedWorkspaceId = TestIds.WorkspaceId.Value;

            ArchiveSourceCommand command = new(requestedWorkspaceId, source.Id.Value);

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(SourceApplicationErrors.NotFoundInWorkspace(source.Id.Value, requestedWorkspaceId));

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WhenSourceIsAlreadyArchived_ShouldReturnFailure()
        {
            // Arrange
            ArchiveSourceHandlerFixture fixture =
                new();

            Source source = fixture.GivenArchivedSourceExists();

            ArchiveSourceCommandHandler handler = fixture.CreateHandler();

            ArchiveSourceCommand command = new(source.WorkspaceId.Value, source.Id.Value);

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(SourceErrors.AlreadyArchived);

            source.Status.Should().Be(SourceStatusType.Archived);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WithEmptyWorkspaceId_ShouldNotQueryRepository()
        {
            // Arrange
            ArchiveSourceHandlerFixture fixture = new();

            ArchiveSourceCommandHandler handler = fixture.CreateHandler();

            ArchiveSourceCommand command = new(Guid.Empty, TestIds.SourceId.Value);

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(WorkspaceApplicationErrors.InvalidId);

            fixture.SourceRepository
                .GetByIdCallCount
                .Should()
                .Be(0);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WithEmptySourceId_ShouldNotQueryRepository()
        {
            // Arrange
            ArchiveSourceHandlerFixture fixture = new();

            ArchiveSourceCommandHandler handler = fixture.CreateHandler();

            ArchiveSourceCommand command = new(TestIds.WorkspaceId.Value, Guid.Empty);

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(SourceApplicationErrors.InvalidId);

            fixture.SourceRepository
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