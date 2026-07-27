using Espada.Domain.Errors;
using Espada.Tests.Domain.TestData.Builders;

namespace Espada.Tests.Domain.Aggregates;

public sealed class ArtifactTests
{
    [Fact]
    public void Create_WithValidArguments_ShouldCreateActiveArtifact()
    {
        // Act
        Artifact artifact = new ArtifactBuilder()
            .WithId(TestIds.DefaultArtifactId)
            .InWorkspace(TestIds.DefaultWorkspaceId)
            .WithTitle("Architecture notes")
            .WithType(ArtifactType.Markdown)
            .CreatedAt(TestDates.ArtifactCreatedAtUtc)
            .Build();

        // Assert
        artifact.Id.Should().Be(TestIds.DefaultArtifactId);
        artifact.WorkspaceId.Should().Be(TestIds.DefaultWorkspaceId);

        artifact.Title.Value.Should().Be("Architecture notes");
        artifact.Type.Should().Be(ArtifactType.Markdown);
        artifact.Status.Should().Be(ArtifactStatusType.Active);
        artifact.Priority.Should().Be(ContextPriority.Neutral);

        artifact.CreatedAtUtc.Should().Be(TestDates.ArtifactCreatedAtUtc);

        artifact.UpdatedAtUtc.Should().Be(TestDates.ArtifactCreatedAtUtc);

        artifact.ArchivedAtUtc.Should().BeNull();
    }

    [Fact]
    public void SetPriority_WhenActive_ShouldUpdatePriorityAndRaiseEvent()
    {
        Artifact artifact = CreateArtifactWithoutPendingEvents();
        ContextPriority priority = ContextPriority.Create(75).ShouldSucceed();

        artifact.SetPriority(priority, TestDates.ArtifactRenamedAtUtc).ShouldSucceed();

        artifact.Priority.Should().Be(priority);
        artifact.UpdatedAtUtc.Should().Be(TestDates.ArtifactRenamedAtUtc);
        artifact.ShouldHaveSingleDomainEvent<ArtifactPriorityChangedDomainEvent>();
    }

    [Fact]
    public void SetPriority_WhenArchived_ShouldFail()
    {
        Artifact artifact = CreateArtifactWithoutPendingEvents();
        artifact.Archive(TestDates.ArtifactArchivedAtUtc).ShouldSucceed();
        artifact.DequeueDomainEvents();

        DomainResult result = artifact.SetPriority(ContextPriority.Create(10).ShouldSucceed(), TestDates.LaterUtc);

        result.ShouldFailWith(ArtifactErrors.ArchivedArtifactCannotChangePriority);
        artifact.Priority.Should().Be(ContextPriority.Neutral);
        artifact.ShouldHaveNoDomainEvents();
    }

    [Fact]
    public void Create_WithValidArguments_ShouldRaiseCreatedEvent()
    {
        // Act
        Artifact artifact = new ArtifactBuilder()
            .WithTitle("Architecture notes")
            .WithType(ArtifactType.Markdown)
            .Build();

        // Assert
        ArtifactCreatedDomainEvent domainEvent = artifact.ShouldHaveSingleDomainEvent<ArtifactCreatedDomainEvent>();

        domainEvent.ArtifactId.Should().Be(TestIds.DefaultArtifactId);

        domainEvent.WorkspaceId.Should().Be(TestIds.DefaultWorkspaceId);

        domainEvent.Title.Should().Be("Architecture notes");
        domainEvent.Type.Should().Be(ArtifactType.Markdown);

        domainEvent.CreatedAtUtc.Should().Be(TestDates.ArtifactCreatedAtUtc);
    }

    [Fact]
    public void Rename_WithDifferentTitle_ShouldChangeTitleAndUpdateTimestamp()
    {
        // Arrange
        Artifact artifact = CreateArtifactWithoutPendingEvents();

        ArtifactTitle newTitle = ArtifactTitle.Create("Updated architecture notes").ShouldSucceed();

        // Act
        DomainResult result = artifact.Rename(newTitle, TestDates.ArtifactRenamedAtUtc);

        // Assert
        result.ShouldSucceed();

        artifact.Title.Should().Be(newTitle);

        artifact.UpdatedAtUtc.Should().Be(TestDates.ArtifactRenamedAtUtc);

        artifact.CreatedAtUtc.Should().Be(TestDates.ArtifactCreatedAtUtc);

        artifact.Status.Should().Be(ArtifactStatusType.Active);
    }

    [Fact]
    public void Rename_WithDifferentTitle_ShouldRaiseRenamedEvent()
    {
        // Arrange
        Artifact artifact = CreateArtifactWithoutPendingEvents();

        ArtifactTitle newTitle = ArtifactTitle.Create("Updated architecture notes").ShouldSucceed();

        // Act
        artifact.Rename(newTitle, TestDates.ArtifactRenamedAtUtc).ShouldSucceed();

        // Assert
        ArtifactRenamedDomainEvent domainEvent = artifact.ShouldHaveSingleDomainEvent<ArtifactRenamedDomainEvent>();

        domainEvent.ArtifactId.Should().Be(TestIds.DefaultArtifactId);

        domainEvent.PreviousTitle.Should().Be("Espada artifact");

        domainEvent.CurrentTitle.Should().Be("Updated architecture notes");

        domainEvent.RenamedAtUtc.Should().Be(TestDates.ArtifactRenamedAtUtc);
    }

    [Fact]
    public void Rename_WithSameTitle_ShouldNotChangeTimestampOrRaiseEvent()
    {
        // Arrange
        Artifact artifact = CreateArtifactWithoutPendingEvents();

        DateTimeOffset originalUpdatedAtUtc = artifact.UpdatedAtUtc;

        ArtifactTitle sameTitle = ArtifactTitle.Create("Espada artifact").ShouldSucceed();

        // Act
        DomainResult result = artifact.Rename(sameTitle, TestDates.ArtifactRenamedAtUtc);

        // Assert
        result.ShouldSucceed();

        artifact.Title.Value.Should().Be("Espada artifact");

        artifact.UpdatedAtUtc.Should().Be(originalUpdatedAtUtc);

        artifact.ShouldHaveNoDomainEvents();
    }

    [Fact]
    public void Rename_WhenArtifactIsArchived_ShouldReturnFailure()
    {
        // Arrange
        Artifact artifact = CreateArtifactWithoutPendingEvents();

        artifact.Archive(TestDates.ArtifactArchivedAtUtc).ShouldSucceed();

        artifact.DequeueDomainEvents();

        ArtifactTitle previousTitle = artifact.Title;
        DateTimeOffset previousUpdatedAtUtc = artifact.UpdatedAtUtc;

        ArtifactTitle newTitle = ArtifactTitle.Create("Forbidden new title").ShouldSucceed();

        // Act
        DomainResult result = artifact.Rename(newTitle, TestDates.LaterUtc);

        // Assert
        result.ShouldFailWith(ArtifactErrors.ArchivedArtifactCannotBeRenamed);

        artifact.Title.Should().Be(previousTitle);

        artifact.UpdatedAtUtc.Should().Be(previousUpdatedAtUtc);

        artifact.Status.Should().Be(ArtifactStatusType.Archived);

        artifact.ShouldHaveNoDomainEvents();
    }

    [Fact]
    public void Archive_WhenArtifactIsActive_ShouldArchiveArtifact()
    {
        // Arrange
        Artifact artifact = CreateArtifactWithoutPendingEvents();

        // Act
        DomainResult result = artifact.Archive(TestDates.ArtifactArchivedAtUtc);

        // Assert
        result.ShouldSucceed();

        artifact.Status.Should().Be(ArtifactStatusType.Archived);

        artifact.ArchivedAtUtc.Should().Be(TestDates.ArtifactArchivedAtUtc);

        artifact.UpdatedAtUtc.Should().Be(TestDates.ArtifactArchivedAtUtc);

        artifact.CreatedAtUtc.Should().Be(TestDates.ArtifactCreatedAtUtc);
    }

    [Fact]
    public void Archive_WhenArtifactIsActive_ShouldRaiseArchivedEvent()
    {
        // Arrange
        Artifact artifact = CreateArtifactWithoutPendingEvents();

        // Act
        artifact.Archive(TestDates.ArtifactArchivedAtUtc).ShouldSucceed();

        // Assert
        ArtifactArchivedDomainEvent domainEvent = artifact.ShouldHaveSingleDomainEvent<ArtifactArchivedDomainEvent>();

        domainEvent.ArtifactId.Should().Be(TestIds.DefaultArtifactId);

        domainEvent.ArchivedAtUtc.Should().Be(TestDates.ArtifactArchivedAtUtc);
    }

    [Fact]
    public void Archive_WhenArtifactIsAlreadyArchived_ShouldReturnFailure()
    {
        // Arrange
        Artifact artifact = CreateArtifactWithoutPendingEvents();

        artifact.Archive(TestDates.ArtifactArchivedAtUtc).ShouldSucceed();

        artifact.DequeueDomainEvents();

        DateTimeOffset? originalArchivedAtUtc = artifact.ArchivedAtUtc;

        DateTimeOffset originalUpdatedAtUtc = artifact.UpdatedAtUtc;

        // Act
        DomainResult result = artifact.Archive(TestDates.LaterUtc);

        // Assert
        result.ShouldFailWith(ArtifactErrors.AlreadyArchived);

        artifact.Status.Should().Be(ArtifactStatusType.Archived);

        artifact.ArchivedAtUtc.Should().Be(originalArchivedAtUtc);

        artifact.UpdatedAtUtc.Should().Be(originalUpdatedAtUtc);

        artifact.ShouldHaveNoDomainEvents();
    }

    private static Artifact CreateArtifactWithoutPendingEvents()
    {
        Artifact artifact = new ArtifactBuilder().Build();

        artifact.DequeueDomainEvents();

        return artifact;
    }
}