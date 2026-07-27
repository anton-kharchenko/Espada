using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Imports.Commands.CancelImport;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.UseCases.Imports.Commands.CancelImport
{
    public sealed class CancelImportCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WhenImportIsRequested_ShouldCancelImport()
        {
            // Arrange
            CancelImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRequestedImportExists();

            CancelImportCommandHandler handler = fixture.CreateHandler();

            CancelImportCommand command = new CancelImportCommandBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldSucceed();

            importJob.Status.Should().Be(ImportStatusType.Cancelled);
            importJob.CompletedAtUtc.Should().Be(TestDates.ImportCancelledAtUtc);
        }

        [Fact]
        public async Task Handle_WhenImportIsRunning_ShouldCancelImport()
        {
            // Arrange
            CancelImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRunningImportExists();

            CancelImportCommandHandler handler = fixture.CreateHandler();

            CancelImportCommand command = new CancelImportCommandBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldSucceed();

            importJob.Status.Should().Be(ImportStatusType.Cancelled);
            importJob.StartedAtUtc.Should().Be(TestDates.ImportStartedAtUtc);
            importJob.CompletedAtUtc.Should().Be(TestDates.ImportCancelledAtUtc);
        }

        [Fact]
        public async Task Handle_WhenImportCanBeCancelled_ShouldUseClockTime()
        {
            // Arrange
            CancelImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRequestedImportExists();

            DateTimeOffset expectedCancelledAtUtc = TestDates.ImportCancelledAtUtc.AddMinutes(10);

            fixture.ClockService.UtcNow = expectedCancelledAtUtc;

            CancelImportCommandHandler handler = fixture.CreateHandler();

            CancelImportCommand command = new CancelImportCommandBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldSucceed();

            importJob.CompletedAtUtc.Should().Be(expectedCancelledAtUtc);
        }

        [Fact]
        public async Task Handle_WhenImportCanBeCancelled_ShouldQueryAndSaveOnce()
        {
            // Arrange
            CancelImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRequestedImportExists();

            CancelImportCommandHandler handler = fixture.CreateHandler();

            CancelImportCommand command = new CancelImportCommandBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

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

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(1);
        }

        [Fact]
        public async Task Handle_ShouldForwardCancellationToken()
        {
            // Arrange
            CancelImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRequestedImportExists();

            CancelImportCommandHandler handler = fixture.CreateHandler();

            CancelImportCommand command = new CancelImportCommandBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

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
            CancelImportHandlerFixture fixture = new();

            fixture.GivenImportDoesNotExist();

            CancelImportCommandHandler handler = fixture.CreateHandler();

            Guid importJobId = TestIds.DefaultImportJobId.Value;

            CancelImportCommand command = new CancelImportCommandBuilder()
                .ForImportJob(importJobId)
                .Build();

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
            CancelImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRequestedImportExists(TestIds.AnotherWorkspaceId);

            CancelImportCommandHandler handler = fixture.CreateHandler();

            Guid requestedWorkspaceId = TestIds.DefaultWorkspaceId.Value;

            CancelImportCommand command = new CancelImportCommandBuilder()
                .InWorkspace(requestedWorkspaceId)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(
                ImportJobApplicationErrors.NotFoundInWorkspace(
                    importJob.Id.Value,
                    requestedWorkspaceId));

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WhenImportIsSucceeded_ShouldReturnCannotCancel()
        {
            // Arrange
            CancelImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenSucceededImportExists();

            DateTimeOffset? originalCompletedAtUtc = importJob.CompletedAtUtc;

            CancelImportCommandHandler handler = fixture.CreateHandler();

            CancelImportCommand command = new CancelImportCommandBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(ImportJobErrors.CannotCancel);

            importJob.Status.Should().Be(ImportStatusType.Succeeded);
            importJob.CompletedAtUtc.Should().Be(originalCompletedAtUtc);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WhenImportIsFailed_ShouldReturnCannotCancel()
        {
            // Arrange
            CancelImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenFailedImportExists();

            DateTimeOffset? originalCompletedAtUtc = importJob.CompletedAtUtc;

            CancelImportCommandHandler handler = fixture.CreateHandler();

            CancelImportCommand command = new CancelImportCommandBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(ImportJobErrors.CannotCancel);

            importJob.Status.Should().Be(ImportStatusType.Failed);
            importJob.CompletedAtUtc.Should().Be(originalCompletedAtUtc);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WhenImportIsAlreadyCancelled_ShouldReturnCannotCancel()
        {
            // Arrange
            CancelImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenCancelledImportExists();

            DateTimeOffset? originalCompletedAtUtc = importJob.CompletedAtUtc;

            CancelImportCommandHandler handler = fixture.CreateHandler();

            CancelImportCommand command = new CancelImportCommandBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(ImportJobErrors.CannotCancel);

            importJob.Status.Should().Be(ImportStatusType.Cancelled);
            importJob.CompletedAtUtc.Should().Be(originalCompletedAtUtc);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WithEmptyWorkspaceId_ShouldNotQueryRepository()
        {
            // Arrange
            CancelImportHandlerFixture fixture = new();

            CancelImportCommandHandler handler = fixture.CreateHandler();

            CancelImportCommand command = new CancelImportCommandBuilder()
                .InWorkspace(Guid.Empty)
                .Build();

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
            CancelImportHandlerFixture fixture = new();

            CancelImportCommandHandler handler = fixture.CreateHandler();

            CancelImportCommand command = new CancelImportCommandBuilder()
                .ForImportJob(Guid.Empty)
                .Build();

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