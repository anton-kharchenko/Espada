using Espada.Domain.Errors;
using Espada.Tests.Domain.TestData.Builders;

namespace Espada.Tests.Domain.Aggregates;

public sealed class ArtifactRevisionTests
{
    [Fact]
    public void CreateRevision_WhenNoRevisionExists_ShouldCreateFirstRevision()
    {
        // Arrange
        Artifact artifact = CreateArtifactWithoutPendingEvents();

        ArtifactContent content = ArtifactContent.Create("# First revision").ShouldSucceed();

        // Act
        DomainResult<ArtifactRevision> result = artifact.CreateRevision(TestIds.FirstRevisionId, content, TestDates.FirstRevisionCreatedAtUtc);

        // Assert
        ArtifactRevision revision = result.ShouldSucceed();

        revision.Id.Should().Be(TestIds.FirstRevisionId);
        revision.ArtifactId.Should().Be(TestIds.DefaultArtifactId);
        revision.Number.Should().Be(RevisionNumber.First());
        revision.Number.Value.Should().Be(1);
        revision.Content.Should().Be(content);
        revision.CreatedAtUtc.Should().Be(TestDates.FirstRevisionCreatedAtUtc);
    }

    [Fact]
    public void CreateRevision_WhenRevisionAlreadyExists_ShouldIncrementRevisionNumber()
    {
        // Arrange
        Artifact artifact = CreateArtifactWithoutPendingEvents();

        ArtifactRevision firstRevision = artifact
            .CreateRevision(TestIds.FirstRevisionId, CreateContent("First revision"), TestDates.FirstRevisionCreatedAtUtc)
            .ShouldSucceed();

        artifact.DequeueDomainEvents();

        // Act
        ArtifactRevision secondRevision = artifact
            .CreateRevision(TestIds.SecondRevisionId, CreateContent("Second revision"), TestDates.SecondRevisionCreatedAtUtc)
            .ShouldSucceed();

        // Assert
        firstRevision.Number.Value.Should().Be(1);
        secondRevision.Number.Value.Should().Be(2);

        secondRevision.ArtifactId.Should().Be(TestIds.DefaultArtifactId);
    }

    [Fact]
    public void CreateRevision_ShouldUpdateCurrentRevisionInformation()
    {
        // Arrange
        Artifact artifact = CreateArtifactWithoutPendingEvents();

        // Act
        ArtifactRevision revision = artifact
            .CreateRevision(TestIds.FirstRevisionId, CreateContent("Revision content"), TestDates.FirstRevisionCreatedAtUtc)
            .ShouldSucceed();

        // Assert
        artifact.CurrentRevisionId.Should().Be(revision.Id);

        artifact.CurrentRevisionNumber
            .Should()
            .Be(revision.Number);

        artifact.RevisionCount.Should().Be(1);

        artifact.UpdatedAtUtc.Should().Be(TestDates.FirstRevisionCreatedAtUtc);

        artifact.CreatedAtUtc.Should().Be(TestDates.ArtifactCreatedAtUtc);
    }

    [Fact]
    public void CreateRevision_WhenCalledTwice_ShouldPointToLatestRevision()
    {
        // Arrange
        Artifact artifact = CreateArtifactWithoutPendingEvents();

        artifact.CreateRevision(TestIds.FirstRevisionId, CreateContent("First revision"), TestDates.FirstRevisionCreatedAtUtc)
            .ShouldSucceed();

        artifact.DequeueDomainEvents();

        // Act
        ArtifactRevision latestRevision = artifact
            .CreateRevision(TestIds.SecondRevisionId, CreateContent("Second revision"), TestDates.SecondRevisionCreatedAtUtc)
            .ShouldSucceed();

        // Assert
        artifact.CurrentRevisionId.Should().Be(TestIds.SecondRevisionId);

        artifact.CurrentRevisionNumber.Should().Be(latestRevision.Number);

        artifact.CurrentRevisionNumber!.Value.Should().Be(2);
        artifact.RevisionCount.Should().Be(2);

        artifact.UpdatedAtUtc.Should().Be(TestDates.SecondRevisionCreatedAtUtc);
    }

    [Fact]
    public void CreateRevision_ShouldCalculateContentHashAndSize()
    {
        // Arrange
        Artifact artifact = CreateArtifactWithoutPendingEvents();

        ArtifactContent content = CreateContent("Hello, Espada!");

        // Act
        ArtifactRevision revision = artifact
            .CreateRevision(TestIds.FirstRevisionId, content, TestDates.FirstRevisionCreatedAtUtc)
            .ShouldSucceed();

        // Assert
        revision.ContentHash.Should().Be(content.Hash);
        revision.ContentHash.Value.Should().NotBeNullOrWhiteSpace();
        revision.ContentHash.Value.Should().HaveLength(64);

        revision.SizeInBytes.Should().Be(content.SizeInBytes);
    }

    [Fact]
    public void CreateRevision_ShouldRaiseRevisionCreatedEvent()
    {
        // Arrange
        Artifact artifact = CreateArtifactWithoutPendingEvents();

        ArtifactContent content = CreateContent("Revision content");

        // Act
        ArtifactRevision revision = artifact
            .CreateRevision(TestIds.FirstRevisionId, content, TestDates.FirstRevisionCreatedAtUtc)
            .ShouldSucceed();

        // Assert
        ArtifactRevisionCreatedDomainEvent domainEvent = artifact.ShouldHaveSingleDomainEvent<ArtifactRevisionCreatedDomainEvent>();

        domainEvent.ArtifactId.Should().Be(TestIds.DefaultArtifactId);
        domainEvent.RevisionId.Should().Be(TestIds.FirstRevisionId);
        domainEvent.RevisionNumber.Should().Be(1);
        domainEvent.ContentHash.Should().Be(revision.ContentHash.Value);
        domainEvent.SizeInBytes.Should().Be(revision.SizeInBytes);
        domainEvent.CreatedAtUtc.Should().Be(TestDates.FirstRevisionCreatedAtUtc);
    }

    [Fact]
    public void CreateRevision_WhenArtifactIsArchived_ShouldReturnFailure()
    {
        // Arrange
        Artifact artifact = CreateArtifactWithoutPendingEvents();

        artifact.Archive(TestDates.ArtifactArchivedAtUtc).ShouldSucceed();

        artifact.DequeueDomainEvents();

        DateTimeOffset originalUpdatedAtUtc = artifact.UpdatedAtUtc;

        // Act
        DomainResult<ArtifactRevision> result = artifact.CreateRevision(TestIds.FirstRevisionId, CreateContent("Forbidden revision"), TestDates.LaterUtc);

        // Assert
        result.ShouldFailWith(ArtifactRevisionErrors.ArtifactArchived);

        artifact.Status.Should().Be(ArtifactStatusType.Archived);

        artifact.CurrentRevisionId.Should().BeNull();
        artifact.CurrentRevisionNumber.Should().BeNull();
        artifact.RevisionCount.Should().Be(0);

        artifact.UpdatedAtUtc.Should().Be(originalUpdatedAtUtc);

        artifact.ShouldHaveNoDomainEvents();
    }

    [Fact]
    public void CreateRevision_WhenArtifactIsArchived_ShouldNotCreateRevision()
    {
        // Arrange
        Artifact artifact = CreateArtifactWithoutPendingEvents();

        artifact.Archive(TestDates.ArtifactArchivedAtUtc).ShouldSucceed();

        artifact.DequeueDomainEvents();

        // Act
        DomainResult<ArtifactRevision> result = artifact.CreateRevision(TestIds.FirstRevisionId, CreateContent("Forbidden revision"), TestDates.LaterUtc);

        // Assert
        result.IsFailure.Should().BeTrue();

        Action accessValue = () =>
        {
            _ = result.Value;
        };

        accessValue
            .Should()
            .Throw<InvalidOperationException>();
    }

    private static Artifact CreateArtifactWithoutPendingEvents()
    {
        Artifact artifact = new ArtifactBuilder().Build();

        artifact.DequeueDomainEvents();

        return artifact;
    }

    private static ArtifactContent CreateContent(string value) => 
        ArtifactContent.Create(value).ShouldSucceed();
}