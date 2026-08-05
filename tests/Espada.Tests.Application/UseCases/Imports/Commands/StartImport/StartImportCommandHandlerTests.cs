using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Imports.Commands.StartImport;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;

namespace Espada.Tests.Application.UseCases.Imports.Commands.StartImport
{
    public sealed class StartImportCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WhenImportIsRequested_ShouldStartImport()
        {
            // Arrange
            StartImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRequestedImportExists();

            StartImportCommandHandler handler = fixture.CreateHandler();

            StartImportCommand command = new(importJob.WorkspaceId.Value, importJob.Id.Value);

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldSucceed();

            importJob.Status.Should().Be(ImportStatusType.Running);

            importJob.StartedAtUtc.Should().Be(TestDates.ImportStartedAtUtc);
        }

        [Fact]
        public async Task Handle_WhenImportIsRequested_ShouldUseClockTime()
        {
            // Arrange
            StartImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRequestedImportExists();

            DateTimeOffset expectedStartedAtUtc = TestDates.ImportStartedAtUtc.AddMinutes(10);

            fixture.ClockService.UtcNow = expectedStartedAtUtc;

            StartImportCommandHandler handler = fixture.CreateHandler();

            StartImportCommand command = new(importJob.WorkspaceId.Value, importJob.Id.Value);

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldSucceed();

            importJob.StartedAtUtc.Should().Be(expectedStartedAtUtc);
        }

        [Fact]
        public async Task Handle_WhenImportIsRequested_ShouldQueryRepositoryOnce()
        {
            // Arrange
            StartImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRequestedImportExists();

            StartImportCommandHandler handler = fixture.CreateHandler();

            StartImportCommand command = new(importJob.WorkspaceId.Value, importJob.Id.Value);

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldSucceed();

            fixture.ImportJobRepository
                .GetByIdCallCount
                .Should()
                .Be(1);

            fixture.ImportJobRepository
                .ReceivedImportJobId
                .Should()
                .Be(importJob.Id);
        }

        [Fact]
        public async Task Handle_WhenImportIsRequested_ShouldSaveChangesOnce()
        {
            // Arrange
            StartImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRequestedImportExists();

            StartImportCommandHandler handler = fixture.CreateHandler();

            StartImportCommand command = new(importJob.WorkspaceId.Value, importJob.Id.Value);

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
            StartImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRequestedImportExists();

            StartImportCommandHandler handler = fixture.CreateHandler();

            StartImportCommand command = new(importJob.WorkspaceId.Value, importJob.Id.Value);

            using CancellationTokenSource tokenSource = new();

            CancellationToken cancellationToken = tokenSource.Token;

            // Act
            DomainResult result = await handler.Handle(command, cancellationToken);

            // Assert
            result.ShouldSucceed();

            fixture.ImportJobRepository
                .GetByIdCancellationToken
                .Should()
                .Be(cancellationToken);

            fixture.UnitOfWork
                .ReceivedCancellationToken
                .Should()
                .Be(cancellationToken);
        }

        [Fact]
        public async Task Handle_WhenImportDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            StartImportHandlerFixture fixture = new();

            fixture.GivenImportDoesNotExist();

            StartImportCommandHandler handler = fixture.CreateHandler();

            Guid importJobId = TestIds.DefaultImportJobId.Value;

            StartImportCommand command = new(TestIds.DefaultWorkspaceId.Value, importJobId);

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(ImportJobApplicationErrors.NotFound(importJobId));

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WhenImportBelongsToAnotherWorkspace_ShouldReturnNotFoundInWorkspace()
        {
            // Arrange
            StartImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRequestedImportExists(TestIds.AnotherWorkspaceId);

            StartImportCommandHandler handler = fixture.CreateHandler();

            Guid requestedWorkspaceId = TestIds.DefaultWorkspaceId.Value;

            StartImportCommand command = new(requestedWorkspaceId, importJob.Id.Value);

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(
                ImportJobApplicationErrors.NotFoundInWorkspace(importJob.Id.Value, requestedWorkspaceId));

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WhenImportIsAlreadyRunning_ShouldReturnFailure()
        {
            // Arrange
            StartImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRunningImportExists();

            StartImportCommandHandler handler = fixture.CreateHandler();

            StartImportCommand command = new(importJob.WorkspaceId.Value, importJob.Id.Value);

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.Should().BeTrue();

            importJob.Status.Should().Be(ImportStatusType.Running);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WithEmptyWorkspaceId_ShouldNotQueryRepository()
        {
            // Arrange
            StartImportHandlerFixture fixture = new();

            StartImportCommandHandler handler = fixture.CreateHandler();

            StartImportCommand command = new(Guid.Empty, TestIds.DefaultImportJobId.Value);

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(WorkspaceApplicationErrors.InvalidId);

            fixture.ImportJobRepository
                .GetByIdCallCount
                .Should()
                .Be(0);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WithEmptyImportJobId_ShouldNotQueryRepository()
        {
            // Arrange
            StartImportHandlerFixture fixture = new();

            StartImportCommandHandler handler = fixture.CreateHandler();

            StartImportCommand command = new(TestIds.DefaultWorkspaceId.Value, Guid.Empty);

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(ImportJobApplicationErrors.InvalidId);

            fixture.ImportJobRepository
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