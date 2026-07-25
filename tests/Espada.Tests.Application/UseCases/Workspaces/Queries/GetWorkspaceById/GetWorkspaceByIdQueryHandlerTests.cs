using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Workspaces.Common;
using Espada.Application.UseCases.Workspaces.Queries.GetWorkspaceById;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Tests.Application.Assertions;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;

namespace Espada.Tests.Application.UseCases.Workspaces.Queries.GetWorkspaceById
{
    public sealed class GetWorkspaceByIdQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WhenWorkspaceExists_ShouldReturnWorkspace()
        {
            // Arrange
            GetWorkspaceByIdHandlerFixture fixture = new();

            Workspace workspace = fixture.GivenWorkspaceExists();

            GetWorkspaceByIdQueryHandler handler = fixture.CreateHandler();

            GetWorkspaceByIdQuery query = new(workspace.Id.Value);

            // Act
            DomainResult<WorkspaceResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            WorkspaceResponse response = result.ShouldSucceed();

            response.Id.Should().Be(workspace.Id.Value);
            response.Name.Should().Be(workspace.Name.Value);
            response.TypeId.Should().Be(workspace.Type.Id);
            response.TypeName.Should().Be(workspace.Type.Name);
            response.StatusId.Should().Be(workspace.Status.Id);
            response.StatusName.Should().Be(workspace.Status.Name);
            response.CreatedAtUtc.Should().Be(workspace.CreatedAtUtc);
        }

        [Fact]
        public async Task Handle_WhenWorkspaceExists_ShouldQueryRepositoryOnce()
        {
            // Arrange
            GetWorkspaceByIdHandlerFixture fixture = new();

            Workspace workspace = fixture.GivenWorkspaceExists();

            GetWorkspaceByIdQueryHandler handler = fixture.CreateHandler();

            GetWorkspaceByIdQuery query = new(workspace.Id.Value);

            // Act
            DomainResult<WorkspaceResponse> result = await handler.Handle(query, CancellationToken.None);

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
        public async Task Handle_ShouldForwardCancellationToken()
        {
            // Arrange
            GetWorkspaceByIdHandlerFixture fixture = new();

            Workspace workspace = fixture.GivenWorkspaceExists();

            GetWorkspaceByIdQueryHandler handler = fixture.CreateHandler();

            GetWorkspaceByIdQuery query = new(workspace.Id.Value);

            using CancellationTokenSource source = new();

            CancellationToken cancellationToken = source.Token;

            // Act
            DomainResult<WorkspaceResponse> result = await handler.Handle(query, cancellationToken);

            // Assert
            result.ShouldSucceed();

            fixture.WorkspaceRepository
                .GetByIdCancellationToken
                .Should()
                .Be(cancellationToken);
        }

        [Fact]
        public async Task Handle_WhenWorkspaceDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            GetWorkspaceByIdHandlerFixture fixture = new();

            fixture.GivenWorkspaceDoesNotExist();

            GetWorkspaceByIdQueryHandler handler = fixture.CreateHandler();

            Guid workspaceId = TestIds.WorkspaceId.Value;

            GetWorkspaceByIdQuery query = new(workspaceId);

            // Act
            DomainResult<WorkspaceResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldFailWith(WorkspaceApplicationErrors.NotFound(workspaceId));
        }

        [Fact]
        public async Task Handle_WhenWorkspaceDoesNotExist_ShouldQueryRepositoryOnce()
        {
            // Arrange
            GetWorkspaceByIdHandlerFixture fixture = new();

            fixture.GivenWorkspaceDoesNotExist();

            GetWorkspaceByIdQueryHandler handler = fixture.CreateHandler();

            GetWorkspaceByIdQuery query = new(TestIds.WorkspaceId.Value);

            // Act
            DomainResult<WorkspaceResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();

            fixture.WorkspaceRepository
                .GetByIdCallCount
                .Should()
                .Be(1);
        }

        [Fact]
        public async Task Handle_WithEmptyWorkspaceId_ShouldReturnInvalidId()
        {
            // Arrange
            GetWorkspaceByIdHandlerFixture fixture = new();

            GetWorkspaceByIdQueryHandler handler = fixture.CreateHandler();

            GetWorkspaceByIdQuery query = new(Guid.Empty);

            // Act
            DomainResult<WorkspaceResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldFailWith(WorkspaceApplicationErrors.InvalidId);

            fixture.WorkspaceRepository
                .GetByIdCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_ShouldNotModifyWorkspace()
        {
            // Arrange
            GetWorkspaceByIdHandlerFixture fixture = new();

            Workspace workspace = fixture.GivenWorkspaceExists();

            int originalDomainEventCount = workspace.DomainEvents.Count;

            GetWorkspaceByIdQueryHandler handler = fixture.CreateHandler();

            GetWorkspaceByIdQuery query = new(workspace.Id.Value);

            // Act
            DomainResult<WorkspaceResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldSucceed();

            workspace.DomainEvents.Should().HaveCount(originalDomainEventCount);
        }
    }
}