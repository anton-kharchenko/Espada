using Espada.Application.ApplicationErrors;
using Espada.Application.UseCases.Sources.Commands.RegisterSource;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.UseCases.Sources.Commands.RegisterSource;

public sealed class RegisterSourceCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenWorkspaceExists_ShouldRegisterSource()
    {
        // Arrange
        RegisterSourceHandlerFixture fixture = new();

        Workspace workspace = fixture.GivenWorkspaceExists();

        RegisterSourceCommandHandler handler = fixture.CreateHandler();

        RegisterSourceCommand command = new RegisterSourceCommandBuilder().InWorkspace(workspace.Id.Value).Build();

        // Act
        DomainResult<RegisterSourceResponse> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        RegisterSourceResponse response = result.ShouldSucceed();

        fixture.SourceRepository
            .AddedSource
            .Should()
            .NotBeNull();

        Source source = fixture.SourceRepository.AddedSource!;

        response.SourceId.Should().Be(source.Id.Value);

        source.WorkspaceId.Should().Be(workspace.Id);

        source.Name.Value.Should().Be(TestValues.SourceName);

        source.Locator.Value.Should().Be(TestValues.SourceLocator);

        source.Type.Should().Be(SourceTypeTestData.Any);
    }

    [Fact]
    public async Task Handle_WhenWorkspaceExists_ShouldUseClockTime()
    {
        // Arrange
        RegisterSourceHandlerFixture fixture = new();

        fixture.GivenWorkspaceExists();

        fixture.Clock.UtcNow = TestDates.LaterUtc;

        RegisterSourceCommandHandler handler = fixture.CreateHandler();

        RegisterSourceCommand command = new RegisterSourceCommandBuilder().Build();

        // Act
        DomainResult<RegisterSourceResponse> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldSucceed();

        Source source = GetAddedSource(fixture);

        source.CreatedAtUtc.Should().Be(TestDates.LaterUtc);
    }

    [Fact]
    public async Task Handle_WhenWorkspaceExists_ShouldPersistAndSaveOnce()
    {
        // Arrange
        RegisterSourceHandlerFixture fixture = new();

        fixture.GivenWorkspaceExists();

        RegisterSourceCommandHandler handler = fixture.CreateHandler();

        RegisterSourceCommand command = new RegisterSourceCommandBuilder().Build();

        // Act
        DomainResult<RegisterSourceResponse> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldSucceed();

        fixture.WorkspaceRepository
            .GetByIdCallCount
            .Should()
            .Be(1);

        fixture.SourceRepository
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
        RegisterSourceHandlerFixture fixture = new();

        fixture.GivenWorkspaceExists();

        RegisterSourceCommandHandler handler = fixture.CreateHandler();

        RegisterSourceCommand command = new RegisterSourceCommandBuilder().Build();

        using CancellationTokenSource source = new();

        CancellationToken cancellationToken = source.Token;

        // Act
        DomainResult<RegisterSourceResponse> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.ShouldSucceed();

        fixture.WorkspaceRepository
            .GetByIdCancellationToken
            .Should()
            .Be(cancellationToken);

        fixture.SourceRepository
            .AddCancellationToken
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
        RegisterSourceHandlerFixture fixture = new();

        fixture.GivenWorkspaceDoesNotExist();

        RegisterSourceCommandHandler handler = fixture.CreateHandler();

        Guid workspaceId = TestIds.DefaultWorkspaceId.Value;

        RegisterSourceCommand command = new RegisterSourceCommandBuilder().InWorkspace(workspaceId).Build();

        // Act
        DomainResult<RegisterSourceResponse> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldFailWith(WorkspaceApplicationErrors.NotFound(workspaceId));

        fixture.SourceRepository
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
        RegisterSourceHandlerFixture fixture = new();

        RegisterSourceCommandHandler handler = fixture.CreateHandler();

        RegisterSourceCommand command = new RegisterSourceCommandBuilder().InWorkspace(Guid.Empty).Build();

        // Act
        DomainResult<RegisterSourceResponse> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldFailWith(WorkspaceApplicationErrors.InvalidId);

        fixture.WorkspaceRepository
            .GetByIdCallCount
            .Should()
            .Be(0);

        fixture.SourceRepository
            .AddCallCount
            .Should()
            .Be(0);

        fixture.UnitOfWork
            .SaveChangesCallCount
            .Should()
            .Be(0);
    }

    [Fact]
    public async Task Handle_WithInvalidName_ShouldNotPersistSource()
    {
        // Arrange
        RegisterSourceHandlerFixture fixture = new();

        fixture.GivenWorkspaceExists();

        RegisterSourceCommandHandler handler = fixture.CreateHandler();

        RegisterSourceCommand command = new RegisterSourceCommandBuilder().WithName(" ").Build();

        // Act
        DomainResult<RegisterSourceResponse> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        fixture.SourceRepository
            .AddCallCount
            .Should()
            .Be(0);

        fixture.UnitOfWork
            .SaveChangesCallCount
            .Should()
            .Be(0);
    }

    [Fact]
    public async Task Handle_WithInvalidLocator_ShouldNotPersistSource()
    {
        // Arrange
        RegisterSourceHandlerFixture fixture = new();

        fixture.GivenWorkspaceExists();

        RegisterSourceCommandHandler handler = fixture.CreateHandler();

        RegisterSourceCommand command = new RegisterSourceCommandBuilder().WithLocator(" ").Build();

        // Act
        DomainResult<RegisterSourceResponse> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        fixture.SourceRepository
            .AddCallCount
            .Should()
            .Be(0);

        fixture.UnitOfWork
            .SaveChangesCallCount
            .Should()
            .Be(0);
    }

    private static Source GetAddedSource(RegisterSourceHandlerFixture fixture)
    {
        fixture.SourceRepository
            .AddedSource
            .Should()
            .NotBeNull();

        return fixture.SourceRepository.AddedSource!;
    }
}