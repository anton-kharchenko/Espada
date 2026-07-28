using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Imports.Commands.CompleteImport;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.UseCases.Imports.Commands.CompleteImport
{
    public sealed class CompleteImportCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WhenImportIsRunning_ShouldCompleteImport()
        {
            // Arrange
            CompleteImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRunningImportExists();

            CompleteImportCommandHandler handler = fixture.CreateHandler();

            CompleteImportCommand command = new CompleteImportCommandBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldSucceed();

            importJob.Status.Should().Be(ImportStatusType.Succeeded);
            importJob.ArtifactId.Should().Be(TestIds.DefaultArtifactId);
            importJob.ArtifactRevisionId.Should().Be(TestIds.DefaultArtifactRevisionId);
            importJob.CompletedAtUtc.Should().Be(TestDates.ImportCompletedAtUtc);
        }

        [Fact]
        public async Task Handle_WhenImportIsRunning_ShouldUseClockTime()
        {
            // Arrange
            CompleteImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRunningImportExists();

            DateTimeOffset expectedCompletedAtUtc = TestDates.ImportCompletedAtUtc.AddMinutes(10);

            fixture.ClockService.UtcNow = expectedCompletedAtUtc;

            CompleteImportCommandHandler handler = fixture.CreateHandler();

            CompleteImportCommand command = new CompleteImportCommandBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldSucceed();

            importJob.CompletedAtUtc.Should().Be(expectedCompletedAtUtc);
        }

        [Fact]
        public async Task Handle_WhenImportIsRunning_ShouldQueryRepositoryOnce()
        {
            // Arrange
            CompleteImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRunningImportExists();

            CompleteImportCommandHandler handler = fixture.CreateHandler();

            CompleteImportCommand command = new CompleteImportCommandBuilder()
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
        }

        [Fact]
        public async Task Handle_WhenImportIsRunning_ShouldSaveChangesOnce()
        {
            // Arrange
            CompleteImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRunningImportExists();

            CompleteImportCommandHandler handler = fixture.CreateHandler();

            CompleteImportCommand command = new CompleteImportCommandBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

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
            CompleteImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRunningImportExists();

            CompleteImportCommandHandler handler = fixture.CreateHandler();

            CompleteImportCommand command = new CompleteImportCommandBuilder()
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
            CompleteImportHandlerFixture fixture = new();

            fixture.GivenImportDoesNotExist();

            CompleteImportCommandHandler handler = fixture.CreateHandler();

            Guid importJobId = TestIds.DefaultImportJobId.Value;

            CompleteImportCommand command = new CompleteImportCommandBuilder()
                .ForImportJob(importJobId)
                .Build();

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(ImportJobApplicationErrors.NotFound(importJobId));

            fixture.ImportJobRepository
                .GetByIdCallCount
                .Should()
                .Be(1);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WhenImportBelongsToAnotherWorkspace_ShouldReturnNotFoundInWorkspace()
        {
            // Arrange
            CompleteImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRunningImportExists(TestIds.AnotherWorkspaceId);

            CompleteImportCommandHandler handler = fixture.CreateHandler();

            Guid requestedWorkspaceId = TestIds.DefaultWorkspaceId.Value;

            CompleteImportCommand command = new CompleteImportCommandBuilder()
                .InWorkspace(requestedWorkspaceId)
                .ForImportJob(importJob.Id.Value)
                .Build();

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
        public async Task Handle_WhenImportIsStillRequested_ShouldReturnFailure()
        {
            // Arrange
            CompleteImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRequestedImportExists();

            CompleteImportCommandHandler handler = fixture.CreateHandler();

            CompleteImportCommand command = new CompleteImportCommandBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.Should().BeTrue();

            importJob.Status.Should().Be(ImportStatusType.Requested);
            importJob.ArtifactId.Should().BeNull();
            importJob.ArtifactRevisionId.Should().BeNull();
            importJob.CompletedAtUtc.Should().BeNull();

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WhenImportIsAlreadySucceeded_ShouldReturnFailure()
        {
            // Arrange
            CompleteImportHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenSucceededImportExists();

            DateTimeOffset? originalCompletedAtUtc = importJob.CompletedAtUtc;

            CompleteImportCommandHandler handler = fixture.CreateHandler();

            CompleteImportCommand command = new CompleteImportCommandBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.Should().BeTrue();

            importJob.Status.Should().Be(ImportStatusType.Succeeded);
            importJob.ArtifactId.Should().Be(TestIds.DefaultArtifactId);
            importJob.ArtifactRevisionId.Should().Be(TestIds.DefaultArtifactRevisionId);
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
            CompleteImportHandlerFixture fixture = new();

            CompleteImportCommandHandler handler = fixture.CreateHandler();

            CompleteImportCommand command = new CompleteImportCommandBuilder()
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
            CompleteImportHandlerFixture fixture = new();

            CompleteImportCommandHandler handler = fixture.CreateHandler();

            CompleteImportCommand command = new CompleteImportCommandBuilder()
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

        [Fact]
        public async Task Handle_WithEmptyArtifactId_ShouldNotQueryRepository()
        {
            // Arrange
            CompleteImportHandlerFixture fixture = new();

            CompleteImportCommandHandler handler = fixture.CreateHandler();

            CompleteImportCommand command = new CompleteImportCommandBuilder()
                .WithArtifact(Guid.Empty)
                .Build();

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(ImportJobApplicationErrors.InvalidArtifactId);

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
        public async Task Handle_WithEmptyArtifactRevisionId_ShouldNotQueryRepository()
        {
            // Arrange
            CompleteImportHandlerFixture fixture = new();

            CompleteImportCommandHandler handler = fixture.CreateHandler();

            CompleteImportCommand command = new CompleteImportCommandBuilder()
                .WithArtifactRevision(Guid.Empty)
                .Build();

            // Act
            DomainResult result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(ImportJobApplicationErrors.InvalidArtifactRevisionId);

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