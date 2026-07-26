using Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Tests.Application.Fixtures;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;

namespace Espada.Tests.Application.UseCases.Workspaces.Commands.CreateWorkspace;

public sealed class CreateWorkspaceCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnCreatedWorkspaceId()
    {
        // Arrange
        CreateWorkspaceHandlerFixture fixture = new();

        CreateWorkspaceCommandHandler handler = fixture.CreateHandler();

        CreateWorkspaceCommand command = new CreateWorkspaceCommandBuilder().Build();

        // Act
        DomainResult<CreateWorkspaceResponse> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        CreateWorkspaceResponse response = result.ShouldSucceed();

        fixture.WorkspaceRepository.AddedWorkspace.Should().NotBeNull();
        Workspace workspace = fixture.WorkspaceRepository.AddedWorkspace!;

        response.WorkspaceId.Should().Be(workspace.Id.Value);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPersistWorkspace()
    {
        // Arrange
        CreateWorkspaceHandlerFixture fixture = new();

        CreateWorkspaceCommandHandler handler = fixture.CreateHandler();

        WorkspaceType workspaceType = WorkspaceTypeTestData.Any;

        CreateWorkspaceCommand command = new CreateWorkspaceCommandBuilder()
                .WithName(TestValues.WorkspaceName)
                .WithType(workspaceType)
                .Build();

        // Act
        DomainResult<CreateWorkspaceResponse> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldSucceed();

        fixture.WorkspaceRepository
            .AddCallCount
            .Should()
            .Be(1);

        fixture.WorkspaceRepository.AddedWorkspace.Should().NotBeNull();
        Workspace workspace = fixture.WorkspaceRepository.AddedWorkspace!;

        workspace.Name.Value.Should().Be(TestValues.WorkspaceName);

        workspace.Type.Should().Be(workspaceType);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUseCurrentClockTime()
    {
        // Arrange
        CreateWorkspaceHandlerFixture fixture = new() { Clock = { UtcNow = TestDates.LaterUtc } };

        CreateWorkspaceCommandHandler handler = fixture.CreateHandler();

        CreateWorkspaceCommand command = new CreateWorkspaceCommandBuilder().Build();

        // Act
        DomainResult<CreateWorkspaceResponse> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldSucceed();

        fixture.WorkspaceRepository.AddedWorkspace.Should().NotBeNull();
        Workspace workspace = fixture.WorkspaceRepository.AddedWorkspace!;

        workspace.CreatedAtUtc.Should().Be(TestDates.LaterUtc);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSaveChangesOnce()
    {
        // Arrange
        CreateWorkspaceHandlerFixture fixture = new();

        CreateWorkspaceCommandHandler handler = fixture.CreateHandler();

        CreateWorkspaceCommand command = new CreateWorkspaceCommandBuilder().Build();

        // Act
        DomainResult<CreateWorkspaceResponse> result = await handler.Handle(command, CancellationToken.None);

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
        CreateWorkspaceHandlerFixture fixture = new();

        CreateWorkspaceCommandHandler handler = fixture.CreateHandler();

        CreateWorkspaceCommand command = new CreateWorkspaceCommandBuilder().Build();

        using CancellationTokenSource source = new();

        CancellationToken cancellationToken = source.Token;

        // Act
        DomainResult<CreateWorkspaceResponse> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.ShouldSucceed();

        fixture.WorkspaceRepository
            .ReceivedCancellationToken
            .Should()
            .Be(cancellationToken);

        fixture.UnitOfWork
            .ReceivedCancellationToken
            .Should()
            .Be(cancellationToken);
    }

    [Fact]
    public async Task Handle_WithInvalidName_ShouldReturnDomainFailure()
    {
        // Arrange
        CreateWorkspaceHandlerFixture fixture = new();

        CreateWorkspaceCommandHandler handler = fixture.CreateHandler();

        CreateWorkspaceCommand command = new CreateWorkspaceCommandBuilder().WithName(" ").Build();

        // Act
        DomainResult<CreateWorkspaceResponse> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldFailWith(WorkspaceErrors.NameEmpty);
    }

    [Fact]
    public async Task Handle_WithInvalidName_ShouldNotPersistAnything()
    {
        // Arrange
        CreateWorkspaceHandlerFixture fixture = new();

        CreateWorkspaceCommandHandler handler = fixture.CreateHandler();

        CreateWorkspaceCommand command = new CreateWorkspaceCommandBuilder().WithName(string.Empty).Build();

        // Act
        DomainResult<CreateWorkspaceResponse> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        fixture.WorkspaceRepository
            .AddCallCount
            .Should()
            .Be(0);

        fixture.WorkspaceRepository
            .AddedWorkspace
            .Should()
            .BeNull();

        fixture.UnitOfWork
            .SaveChangesCallCount
            .Should()
            .Be(0);
    }
}