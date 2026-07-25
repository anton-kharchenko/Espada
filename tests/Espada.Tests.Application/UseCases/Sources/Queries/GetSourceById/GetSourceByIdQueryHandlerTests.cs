using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Sources.Common;
using Espada.Application.UseCases.Sources.Queries.GetSourceById;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Tests.Application.Assertions;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;

namespace Espada.Tests.Application.UseCases.Sources.Queries.GetSourceById
{
    public sealed class GetSourceByIdQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WhenSourceExists_ShouldReturnSource()
        {
            // Arrange
            GetSourceByIdHandlerFixture fixture = new();

            Source source = fixture.GivenSourceExists();

            GetSourceByIdQueryHandler handler = fixture.CreateHandler();

            GetSourceByIdQuery query = new(source.WorkspaceId.Value, source.Id.Value);

            // Act
            DomainResult<SourceResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            SourceResponse response = result.ShouldSucceed();

            response.Id.Should().Be(source.Id.Value);
            response.WorkspaceId.Should().Be(source.WorkspaceId.Value);
            response.Name.Should().Be(source.Name.Value);
            response.Locator.Should().Be(source.Locator.Value);
            response.TypeId.Should().Be(source.Type.Id);
            response.TypeName.Should().Be(source.Type.Name);
            response.StatusId.Should().Be(source.Status.Id);
            response.StatusName.Should().Be(source.Status.Name);
            response.CreatedAtUtc.Should().Be(source.CreatedAtUtc);
        }

        [Fact]
        public async Task Handle_WhenSourceExists_ShouldQueryRepositoryOnce()
        {
            // Arrange
            GetSourceByIdHandlerFixture fixture = new();

            Source source = fixture.GivenSourceExists();

            GetSourceByIdQueryHandler handler = fixture.CreateHandler();

            GetSourceByIdQuery query = new(source.WorkspaceId.Value, source.Id.Value);

            // Act
            DomainResult<SourceResponse> result = await handler.Handle(query, CancellationToken.None);

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
        public async Task Handle_ShouldForwardCancellationToken()
        {
            // Arrange
            GetSourceByIdHandlerFixture fixture = new();

            Source source = fixture.GivenSourceExists();

            GetSourceByIdQueryHandler handler = fixture.CreateHandler();

            GetSourceByIdQuery query = new(source.WorkspaceId.Value, source.Id.Value);

            using CancellationTokenSource sourceToken = new();

            CancellationToken cancellationToken = sourceToken.Token;

            // Act
            DomainResult<SourceResponse> result = await handler.Handle(query, cancellationToken);

            // Assert
            result.ShouldSucceed();

            fixture.SourceRepository
                .GetByIdCancellationToken
                .Should()
                .Be(cancellationToken);
        }

        [Fact]
        public async Task Handle_WhenSourceDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            GetSourceByIdHandlerFixture fixture = new();

            fixture.GivenSourceDoesNotExist();

            GetSourceByIdQueryHandler handler = fixture.CreateHandler();

            Guid sourceId = TestIds.SourceId.Value;

            GetSourceByIdQuery query = new(TestIds.WorkspaceId.Value, sourceId);

            // Act
            DomainResult<SourceResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldFailWith(SourceApplicationErrors.NotFound(sourceId));
        }

        [Fact]
        public async Task Handle_WhenSourceBelongsToAnotherWorkspace_ShouldReturnNotFoundInWorkspace()
        {
            // Arrange
            GetSourceByIdHandlerFixture fixture = new();

            Source source = fixture.GivenSourceExists(TestIds.AnotherWorkspaceId);

            GetSourceByIdQueryHandler handler = fixture.CreateHandler();

            Guid requestedWorkspaceId = TestIds.WorkspaceId.Value;

            GetSourceByIdQuery query = new(requestedWorkspaceId, source.Id.Value);

            // Act
            DomainResult<SourceResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldFailWith(SourceApplicationErrors.NotFoundInWorkspace(source.Id.Value, requestedWorkspaceId));
        }

        [Fact]
        public async Task Handle_WithEmptyWorkspaceId_ShouldNotQueryRepository()
        {
            // Arrange
            GetSourceByIdHandlerFixture fixture = new();

            GetSourceByIdQueryHandler handler = fixture.CreateHandler();

            GetSourceByIdQuery query = new(Guid.Empty, TestIds.SourceId.Value);

            // Act
            DomainResult<SourceResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldFailWith(WorkspaceApplicationErrors.InvalidId);

            fixture.SourceRepository
                .GetByIdCallCount
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task Handle_WithEmptySourceId_ShouldNotQueryRepository()
        {
            // Arrange
            GetSourceByIdHandlerFixture fixture = new();

            GetSourceByIdQueryHandler handler = fixture.CreateHandler();

            GetSourceByIdQuery query = new(TestIds.WorkspaceId.Value, Guid.Empty);

            // Act
            DomainResult<SourceResponse> result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldFailWith(SourceApplicationErrors.InvalidId);

            fixture.SourceRepository
                .GetByIdCallCount
                .Should()
                .Be(0);
        }
    }
}