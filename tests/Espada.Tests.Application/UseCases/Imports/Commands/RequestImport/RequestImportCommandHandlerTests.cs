using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Tests.Application.Assertions;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.UseCases.Imports.Commands.RequestImport
{
    public sealed class RequestImportCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WhenSourceExists_ShouldCreateImportJob()
        {
            // Arrange
            RequestImportHandlerFixture fixture = new();

            Source source = fixture.GivenSourceExists();

            RequestImportCommandHandler handler = fixture.CreateHandler();

            RequestImportCommand command = new RequestImportCommandBuilder()
                .InWorkspace(source.WorkspaceId.Value)
                    .ForSource(source.Id.Value)
                    .Build();

            // Act
            DomainResult<RequestImportResponse> result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            RequestImportResponse response = result.ShouldSucceed();

            ImportJob importJob = GetAddedImportJob(fixture);

            response.ImportJobId.Should().Be(importJob.Id.Value);
            importJob.WorkspaceId.Should().Be(source.WorkspaceId);
            importJob.SourceId.Should().Be(source.Id);
            importJob.Status.Should().Be(ImportStatusType.Requested);
        }

        [Fact]
        public async Task Handle_WhenSourceExists_ShouldUseClockTime()
        {
            // Arrange
            RequestImportHandlerFixture fixture = new();

            fixture.GivenSourceExists();

            fixture.Clock.UtcNow = TestDates.ImportRequestedAtUtc;

            RequestImportCommandHandler handler = fixture.CreateHandler();

            RequestImportCommand command = new RequestImportCommandBuilder().Build();

            // Act
            DomainResult<RequestImportResponse> result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldSucceed();

            ImportJob importJob = GetAddedImportJob(fixture);

            importJob.RequestedAtUtc.Should().Be(TestDates.ImportRequestedAtUtc);
        }

        [Fact]
        public async Task Handle_WhenSourceExists_ShouldPersistAndSaveOnce()
        {
            // Arrange
            RequestImportHandlerFixture fixture = new();

            fixture.GivenSourceExists();

            RequestImportCommandHandler handler = fixture.CreateHandler();

            RequestImportCommand command = new RequestImportCommandBuilder().Build();

            // Act
            DomainResult<RequestImportResponse> result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldSucceed();

            fixture.SourceRepository
                .GetByIdCallCount
                .Should()
                .Be(1);

            fixture.ImportJobRepository
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
            RequestImportHandlerFixture fixture = new();

            fixture.GivenSourceExists();

            RequestImportCommandHandler handler = fixture.CreateHandler();

            RequestImportCommand command = new RequestImportCommandBuilder().Build();

            using CancellationTokenSource tokenSource = new();

            CancellationToken cancellationToken = tokenSource.Token;

            // Act
            DomainResult<RequestImportResponse> result = await handler.Handle(command, cancellationToken);

            // Assert
            result.ShouldSucceed();

            fixture.SourceRepository
                .GetByIdCancellationToken
                .Should()
                .Be(cancellationToken);

            fixture.ImportJobRepository
                .AddCancellationToken
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
            RequestImportHandlerFixture fixture = new();

            fixture.GivenSourceDoesNotExist();

            RequestImportCommandHandler handler = fixture.CreateHandler();

            Guid sourceId = TestIds.SourceId.Value;

            RequestImportCommand command = new RequestImportCommandBuilder()
                    .ForSource(sourceId)
                    .Build();

            // Act
            DomainResult<RequestImportResponse> result =
                await handler.Handle(
                    command,
                    TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(SourceApplicationErrors.NotFound(sourceId));

            fixture.ImportJobRepository
                .AddCallCount
                .Should()
                .Be(0);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WhenSourceBelongsToAnotherWorkspace_ShouldReturnNotFoundInWorkspace()
        {
            // Arrange
            RequestImportHandlerFixture fixture = new();

            Source source = fixture.GivenSourceExists(TestIds.AnotherWorkspaceId);

            RequestImportCommandHandler handler = fixture.CreateHandler();

            Guid requestedWorkspaceId =
                TestIds.DefaultWorkspaceId.Value;

            RequestImportCommand command = new RequestImportCommandBuilder()
                    .InWorkspace(requestedWorkspaceId)
                    .ForSource(source.Id.Value)
                    .Build();

            // Act
            DomainResult<RequestImportResponse> result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(SourceApplicationErrors.NotFoundInWorkspace(source.Id.Value, requestedWorkspaceId));

            fixture.ImportJobRepository
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
            RequestImportHandlerFixture fixture = new();

            RequestImportCommandHandler handler = fixture.CreateHandler();

            RequestImportCommand command = new RequestImportCommandBuilder()
                    .InWorkspace(Guid.Empty)
                    .Build();

            // Act
            DomainResult<RequestImportResponse> result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(WorkspaceApplicationErrors.InvalidId);

            fixture.SourceRepository
                .GetByIdCallCount
                .Should()
                .Be(0);

            fixture.ImportJobRepository
                .AddCallCount
                .Should()
                .Be(0);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WithEmptySourceId_ShouldNotQueryOrPersist()
        {
            // Arrange
            RequestImportHandlerFixture fixture = new();

            RequestImportCommandHandler handler = fixture.CreateHandler();

            RequestImportCommand command = new RequestImportCommandBuilder()
                .ForSource(Guid.Empty)
                .Build();

            // Act
            DomainResult<RequestImportResponse> result = await handler.Handle(command, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(SourceApplicationErrors.InvalidId);

            fixture.SourceRepository
                .GetByIdCallCount
                .Should()
                .Be(0);

            fixture.ImportJobRepository
                .AddCallCount
                .Should()
                .Be(0);

            fixture.UnitOfWork
                .SaveChangesCallCount
                .Should()
                .Be(0);
        }

        private static ImportJob GetAddedImportJob(RequestImportHandlerFixture fixture)
        {
            fixture.ImportJobRepository
                .AddedImportJob
                .Should()
                .NotBeNull();

            return fixture.ImportJobRepository.AddedImportJob!;
        }
    }
}