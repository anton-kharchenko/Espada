using Espada.Tests.Domain.TestData.Builders;

namespace Espada.Tests.Domain.Aggregates;

public sealed class WorkspaceTests
{
    [Fact]
    public void Create_WithValidArguments_ShouldCreateActiveWorkspace()
    {
        // Arrange
        WorkspaceId id = TestIds.DefaultWorkspaceId;
        WorkspaceName? name = WorkspaceName.Create("Espada Team").Value;
        DateTimeOffset createdAtUtc = new(2026, 7, 24, 10, 30, 0, TimeSpan.Zero);

        // Act
        DomainResult<Workspace> result = Workspace.Create(
            id,
            name ?? throw new InvalidOperationException("Name cannot be null."),
            WorkspaceType.Team,
            null,
            createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();

        Workspace? workspace = result.Value;

        workspace?.Id.Should().Be(id);
        workspace?.Name.Should().Be(name);
        workspace?.Type.Should().Be(WorkspaceType.Team);
        workspace?.OrganizationId.Should().BeNull();
        workspace?.Status.Should().Be(WorkspaceStatusType.Active);
        workspace?.CreatedAtUtc.Should().Be(createdAtUtc);
        workspace?.ArchivedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_WithValidArguments_ShouldRaiseCreatedEvent()
    {
        // Arrange
        DateTimeOffset createdAtUtc = TestDates.CreatedAtUtc;

        // Act
        Workspace workspace = new WorkspaceBuilder()
            .WithId(TestIds.DefaultWorkspaceId)
            .WithName("Espada Team")
            .WithType(WorkspaceType.Team)
            .CreatedAt(createdAtUtc)
            .Build();

        // Assert
        WorkspaceCreatedDomainEvent domainEvent = workspace.ShouldHaveSingleDomainEvent<WorkspaceCreatedDomainEvent>();

        domainEvent.WorkspaceId.Should().Be(TestIds.DefaultWorkspaceId);
        domainEvent.Name.Should().Be("Espada Team");
        domainEvent.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void DequeueDomainEvents_ShouldReturnAndClearPendingEvents()
    {
        // Arrange
        Workspace workspace = new WorkspaceBuilder().Build();

        // Act
        IReadOnlyCollection<IDomainEvent> events = workspace.DequeueDomainEvents();

        // Assert
        events.Should().ContainSingle();
        events.Single().Should().BeOfType<WorkspaceCreatedDomainEvent>();

        workspace.ShouldHaveNoDomainEvents();
    }

    [Fact]
    public void DequeueDomainEvents_WhenNoEventsRemain_ShouldReturnEmptyCollection()
    {
        // Arrange
        Workspace workspace = new WorkspaceBuilder().Build();

        workspace.DequeueDomainEvents();

        // Act
        IReadOnlyCollection<IDomainEvent> events = workspace.DequeueDomainEvents();

        // Assert
        events.Should().BeEmpty();
    }

    [Fact]
    public void Equality_WithSameIdentity_ShouldConsiderWorkspacesEqual()
    {
        // Arrange
        Workspace first = new WorkspaceBuilder()
            .WithId(TestIds.DefaultWorkspaceId)
            .WithName("First name")
            .Build();

        Workspace second = new WorkspaceBuilder()
            .WithId(TestIds.DefaultWorkspaceId)
            .WithName("Second name")
            .Build();

        // Assert
        first.Should().Be(second);
        first.GetHashCode().Should().Be(second.GetHashCode());

        (first == second).Should().BeTrue();
        (first != second).Should().BeFalse();
    }

    [Fact]
    public void Equality_WithDifferentIdentity_ShouldConsiderWorkspacesDifferent()
    {
        // Arrange
        Workspace first = new WorkspaceBuilder()
            .WithId(TestIds.DefaultWorkspaceId)
            .Build();

        Workspace second = new WorkspaceBuilder()
            .WithId(TestIds.AnotherWorkspaceId)
            .Build();

        // Assert
        first.Should().NotBe(second);
        (first == second).Should().BeFalse();
        (first != second).Should().BeTrue();
    }

    [Fact]
    public void Archive_WhenWorkspaceIsActive_ShouldRaiseArchivedEvent()
    {
        // Arrange
        Workspace workspace = new WorkspaceBuilder().BuildWithoutPendingEvents();

        // Act
        workspace.Archive(TestDates.ArchivedAtUtc).ShouldSucceed();

        // Assert
        WorkspaceArchivedDomainEvent domainEvent = workspace.ShouldHaveSingleDomainEvent<WorkspaceArchivedDomainEvent>();

        domainEvent.WorkspaceId.Should().Be(workspace.Id);

        domainEvent.ArchivedAtUtc.Should().Be(TestDates.ArchivedAtUtc);
    }
}
