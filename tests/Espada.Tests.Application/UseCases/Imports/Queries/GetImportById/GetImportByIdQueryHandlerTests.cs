using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Imports.Queries.GetImportById;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.UseCases.Imports.Queries.GetImportById
{
    public sealed class GetImportByIdQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WhenImportExists_ShouldReturnImport()
        {
            // Arrange
            GetImportByIdHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRequestedImportExists();

            GetImportByIdQueryHandler handler = fixture.CreateHandler();

            GetImportByIdQuery query = new GetImportByIdQueryBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult<GetImportByIdResponse> result = await handler.Handle(query, TestContext.Current.CancellationToken);

            // Assert
            GetImportByIdResponse response = result.ShouldSucceed();

            response.Id.Should().Be(importJob.Id.Value);
            response.SourceId.Should().Be(importJob.SourceId.Value);
            response.WorkspaceId.Should().Be(importJob.WorkspaceId.Value);
            response.StatusId.Should().Be(ImportStatusType.Requested.Id);
            response.StatusName.Should().Be(ImportStatusType.Requested.Name);
            response.RequestedAtUtc.Should().Be(importJob.RequestedAtUtc);
            response.StartedAtUtc.Should().BeNull();
            response.CompletedAtUtc.Should().BeNull();
            response.ArtifactId.Should().BeNull();
            response.ArtifactRevisionId.Should().BeNull();
            response.FailureCode.Should().BeNull();
            response.FailureReason.Should().BeNull();
        }

        [Fact]
        public async Task Handle_WhenImportSucceeded_ShouldReturnArtifactReferences()
        {
            // Arrange
            GetImportByIdHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenSucceededImportExists();

            GetImportByIdQueryHandler handler = fixture.CreateHandler();

            GetImportByIdQuery query = new GetImportByIdQueryBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult<GetImportByIdResponse> result = await handler.Handle(query, TestContext.Current.CancellationToken);

            // Assert
            GetImportByIdResponse response = result.ShouldSucceed();

            response.StatusId.Should().Be(ImportStatusType.Succeeded.Id);
            response.StatusName.Should().Be(ImportStatusType.Succeeded.Name);
            response.StartedAtUtc.Should().Be(TestDates.ImportStartedAtUtc);
            response.CompletedAtUtc.Should().Be(TestDates.ImportCompletedAtUtc);
            response.ArtifactId.Should().Be(TestIds.DefaultArtifactId.Value);
            response.ArtifactRevisionId.Should().Be(TestIds.DefaultArtifactRevisionId.Value);
            response.FailureCode.Should().BeNull();
            response.FailureReason.Should().BeNull();
        }

        [Fact]
        public async Task Handle_WhenImportFailed_ShouldReturnFailure()
        {
            // Arrange
            GetImportByIdHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenFailedImportExists();

            GetImportByIdQueryHandler handler = fixture.CreateHandler();

            GetImportByIdQuery query = new GetImportByIdQueryBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult<GetImportByIdResponse> result = await handler.Handle(query, TestContext.Current.CancellationToken);

            // Assert
            GetImportByIdResponse response = result.ShouldSucceed();

            response.StatusId.Should().Be(ImportStatusType.Failed.Id);
            response.StatusName.Should().Be(ImportStatusType.Failed.Name);
            response.StartedAtUtc.Should().Be(TestDates.ImportStartedAtUtc);
            response.CompletedAtUtc.Should().Be(TestDates.ImportFailedAtUtc);
            response.ArtifactId.Should().BeNull();
            response.ArtifactRevisionId.Should().BeNull();
            response.FailureCode.Should().Be(TestValues.ImportFailureCode);
            response.FailureReason.Should().Be(TestValues.ImportFailureReason);
        }

        [Fact]
        public async Task Handle_WhenImportExists_ShouldQueryRepositoryOnce()
        {
            // Arrange
            GetImportByIdHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRequestedImportExists();

            GetImportByIdQueryHandler handler = fixture.CreateHandler();

            GetImportByIdQuery query = new GetImportByIdQueryBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult<GetImportByIdResponse> result = await handler.Handle(query, TestContext.Current.CancellationToken);

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
        public async Task Handle_ShouldForwardCancellationToken()
        {
            // Arrange
            GetImportByIdHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRequestedImportExists();

            GetImportByIdQueryHandler handler = fixture.CreateHandler();

            GetImportByIdQuery query = new GetImportByIdQueryBuilder()
                .InWorkspace(importJob.WorkspaceId.Value)
                .ForImportJob(importJob.Id.Value)
                .Build();

            using CancellationTokenSource tokenSource = new();

            CancellationToken cancellationToken = tokenSource.Token;

            // Act
            DomainResult<GetImportByIdResponse> result = await handler.Handle(query, cancellationToken);

            // Assert
            result.ShouldSucceed();

            fixture.ImportJobRepository
                .GetByIdCancellationToken
                .Should()
                .Be(cancellationToken);
        }

        [Fact]
        public async Task Handle_WhenImportDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            GetImportByIdHandlerFixture fixture = new();

            fixture.GivenImportDoesNotExist();

            GetImportByIdQueryHandler handler = fixture.CreateHandler();

            Guid importJobId = TestIds.DefaultImportJobId.Value;

            GetImportByIdQuery query = new GetImportByIdQueryBuilder()
                .ForImportJob(importJobId)
                .Build();

            // Act
            DomainResult<GetImportByIdResponse> result = await handler.Handle(
                query,
                TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(ImportJobApplicationErrors.NotFound(importJobId));
        }

        [Fact]
        public async Task Handle_WhenImportBelongsToAnotherWorkspace_ShouldReturnNotFoundInWorkspace()
        {
            // Arrange
            GetImportByIdHandlerFixture fixture = new();

            ImportJob importJob = fixture.GivenRequestedImportExists(TestIds.AnotherWorkspaceId);

            GetImportByIdQueryHandler handler = fixture.CreateHandler();

            Guid requestedWorkspaceId = TestIds.DefaultWorkspaceId.Value;

            GetImportByIdQuery query = new GetImportByIdQueryBuilder()
                .InWorkspace(requestedWorkspaceId)
                .ForImportJob(importJob.Id.Value)
                .Build();

            // Act
            DomainResult<GetImportByIdResponse> result = await handler.Handle(query, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(ImportJobApplicationErrors.NotFoundInWorkspace(importJob.Id.Value, requestedWorkspaceId));
        }

        [Fact]
        public async Task Handle_WithEmptyWorkspaceId_ShouldNotQueryRepository()
        {
            // Arrange
            GetImportByIdHandlerFixture fixture = new();

            GetImportByIdQueryHandler handler = fixture.CreateHandler();

            GetImportByIdQuery query = new GetImportByIdQueryBuilder()
                .InWorkspace(Guid.Empty)
                .Build();

            // Act
            DomainResult<GetImportByIdResponse> result = await handler.Handle(query, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(WorkspaceApplicationErrors.InvalidId);

            fixture.ImportJobRepository
                .GetByIdCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WithEmptyImportJobId_ShouldNotQueryRepository()
        {
            // Arrange
            GetImportByIdHandlerFixture fixture = new();

            GetImportByIdQueryHandler handler = fixture.CreateHandler();

            GetImportByIdQuery query = new GetImportByIdQueryBuilder()
                .ForImportJob(Guid.Empty)
                .Build();

            // Act
            DomainResult<GetImportByIdResponse> result = await handler.Handle(query, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldFailWith(ImportJobApplicationErrors.InvalidId);

            fixture.ImportJobRepository
                .GetByIdCallCount
                .Should()
                .Be(0);
        }
    }
}