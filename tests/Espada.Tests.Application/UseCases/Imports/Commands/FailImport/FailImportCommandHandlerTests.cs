using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Imports.Commands.FailImport;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.UseCases.Imports.Commands.FailImport
{
    public sealed class FailImportCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WhenImportIsRunning_ShouldFailImport()
        {
            // Arrange
            FailImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRunningImportExists();

            FailImportCommandHandler handler = fixture.CreateHandler();

            FailImportCommand command = new FailImportCommandBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            // Assert
            result.ShouldSucceed();

            importJob.Status.Should().Be(ImportStatusType.Failed);
            importJob.Failure.Should().NotBeNull();
            importJob.Failure!.Code.Should().Be(TestValues.ImportFailureCode);
            importJob.Failure.Reason.Should().Be(TestValues.ImportFailureReason);
            importJob.CompletedAtUtc.Should().Be(TestDates.ImportFailedAtUtc);
        }

        [Fact]
        public async Task Handle_WhenImportIsRunning_ShouldUseClockTime()
        {
            // Arrange
            FailImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRunningImportExists();

            DateTimeOffset expectedFailedAtUtc = TestDates.ImportFailedAtUtc.AddMinutes(10);

            fixture.Clock.UtcNow = expectedFailedAtUtc;

            FailImportCommandHandler handler = fixture.CreateHandler();

            FailImportCommand command = new FailImportCommandBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            // Assert
            result.ShouldSucceed();

            importJob.CompletedAtUtc.Should().Be(expectedFailedAtUtc);
        }

        [Fact]
        public async Task Handle_WhenImportIsRunning_ShouldQueryAndSaveOnce()
        {
            // Arrange
            FailImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRunningImportExists();

            FailImportCommandHandler handler = fixture.CreateHandler();

            FailImportCommand command = new FailImportCommandBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

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
            FailImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRunningImportExists();

            FailImportCommandHandler handler = fixture.CreateHandler();

            FailImportCommand command = new FailImportCommandBuilder()
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
            FailImportHandlerFixture fixture = new();

            fixture.GivenImportDoesNotExist();

            FailImportCommandHandler handler = fixture.CreateHandler();

            Guid importJobId = TestIds.DefaultImportJobId.Value;

            FailImportCommand command = new FailImportCommandBuilder()
                .ForImportJob(importJobId)
                .Build();

            // Act
            DomainResult result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

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
            FailImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRunningImportExists(TestIds.AnotherWorkspaceId);

            FailImportCommandHandler handler = fixture.CreateHandler();

            Guid requestedWorkspaceId = TestIds.DefaultWorkspaceId.Value;

            FailImportCommand command = new FailImportCommandBuilder()
                .InWorkspace(requestedWorkspaceId)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

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
        public async Task Handle_WhenImportIsRequested_ShouldReturnFailure()
        {
            // Arrange
            FailImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRequestedImportExists();

            FailImportCommandHandler handler = fixture.CreateHandler();

            FailImportCommand command = new FailImportCommandBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.Should().BeTrue();

            importJob.Status.Should().Be(ImportStatusType.Requested);
            importJob.Failure.Should().BeNull();
            importJob.CompletedAtUtc.Should().BeNull();

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WhenImportIsAlreadyFailed_ShouldReturnFailure()
        {
            // Arrange
            FailImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenFailedImportExists();

            FailImportCommandHandler handler = fixture.CreateHandler();

            FailImportCommand command = new FailImportCommandBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .WithFailureCode(TestValues.AnotherImportFailureCode)
                .WithFailureReason(TestValues.AnotherImportFailureReason)
                .Build();

            // Act
            DomainResult result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.Should().BeTrue();

            importJob.Status.Should().Be(ImportStatusType.Failed);
            importJob.Failure!.Code.Should().Be(TestValues.ImportFailureCode);
            importJob.Failure.Reason.Should().Be(TestValues.ImportFailureReason);
            importJob.CompletedAtUtc.Should().Be(TestDates.ImportFailedAtUtc);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WithEmptyWorkspaceId_ShouldNotQueryRepository()
        {
            // Arrange
            FailImportHandlerFixture fixture = new();

            FailImportCommandHandler handler = fixture.CreateHandler();

            FailImportCommand command = new FailImportCommandBuilder()
                .InWorkspace(Guid.Empty)
                .Build();

            // Act
            DomainResult result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

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
            FailImportHandlerFixture fixture = new();

            FailImportCommandHandler handler = fixture.CreateHandler();

            FailImportCommand command = new FailImportCommandBuilder()
                .ForImportJob(Guid.Empty)
                .Build();

            // Act
            DomainResult result = await handler.Handle(
                command,
                TestContext.Current.CancellationToken);

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