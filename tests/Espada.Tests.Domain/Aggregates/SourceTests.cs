using Espada.Domain.Errors;
using Espada.Tests.Domain.TestData.Builders;

namespace Espada.Tests.Domain.Aggregates
{
    public sealed class SourceTests
    {
        [Fact]
        public void Create_WithValidArguments_ShouldCreateActiveSource()
        {
            // Act
            Source source = new SourceBuilder()
                .WithName("Product documentation")
                .WithType(SourceType.WebPage)
                .WithLocator("https://example.com/docs")
                .Build();

            // Assert
            source.Id.Should().Be(TestIds.DefaultSourceId);

            source.WorkspaceId.Should().Be(TestIds.DefaultWorkspaceId);

            source.Name.Value.Should().Be("Product documentation");

            source.Type.Should().Be(SourceType.WebPage);

            source.Locator.Value.Should().Be("https://example.com/docs");

            source.Status.Should().Be(SourceStatusType.Active);
            source.Priority.Should().Be(ContextPriority.Neutral);

            source.CreatedAtUtc.Should().Be(TestDates.SourceCreatedAtUtc);

            source.UpdatedAtUtc.Should().Be(TestDates.SourceCreatedAtUtc);

            source.ArchivedAtUtc.Should().BeNull();
        }

        [Fact]
        public void SetPriority_WhenActive_ShouldUpdatePriorityAndRaiseEvent()
        {
            Source source = new SourceBuilder().BuildWithoutPendingEvents();
            ContextPriority priority = ContextPriority.Create(-40).ShouldSucceed();

            source.SetPriority(priority, TestDates.SourceArchivedAtUtc).ShouldSucceed();

            source.Priority.Should().Be(priority);
            source.UpdatedAtUtc.Should().Be(TestDates.SourceArchivedAtUtc);
            source.ShouldHaveSingleDomainEvent<SourcePriorityChangedDomainEvent>();
        }

        [Fact]
        public void SetPriority_WhenArchived_ShouldFail()
        {
            Source source = new SourceBuilder().BuildWithoutPendingEvents();
            source.Archive(TestDates.SourceArchivedAtUtc).ShouldSucceed();
            source.DequeueDomainEvents();

            DomainResult result = source.SetPriority(ContextPriority.Create(10).ShouldSucceed(), TestDates.LaterUtc);

            result.ShouldFailWith(SourceErrors.ArchivedSourceCannotChangePriority);
            source.Priority.Should().Be(ContextPriority.Neutral);
            source.ShouldHaveNoDomainEvents();
        }

        [Fact]
        public void Create_WithValidArguments_ShouldRaiseCreatedEvent()
        {
            // Act
            Source source = new SourceBuilder()
                .WithName("Product documentation")
                .WithType(SourceType.WebPage)
                .WithLocator("https://example.com/docs")
                .Build();

            // Assert
            SourceCreatedDomainEvent domainEvent = source.ShouldHaveSingleDomainEvent<SourceCreatedDomainEvent>();

            domainEvent.SourceId.Should().Be(TestIds.DefaultSourceId);

            domainEvent.WorkspaceId.Should().Be(TestIds.DefaultWorkspaceId);

            domainEvent.Name.Should().Be("Product documentation");

            domainEvent.Type.Should().Be(SourceType.WebPage);

            domainEvent.Locator.Should().Be("https://example.com/docs");

            domainEvent.CreatedAtUtc.Should().Be(TestDates.SourceCreatedAtUtc);
        }

        [Fact]
        public void Archive_WhenSourceIsActive_ShouldArchiveSource()
        {
            // Arrange
            Source source = new SourceBuilder().BuildWithoutPendingEvents();

            // Act
            DomainResult result = source.Archive(TestDates.SourceArchivedAtUtc);

            // Assert
            result.ShouldSucceed();

            source.Status.Should().Be(SourceStatusType.Archived);

            source.ArchivedAtUtc.Should().Be(TestDates.SourceArchivedAtUtc);

            source.UpdatedAtUtc.Should().Be(TestDates.SourceArchivedAtUtc);

            source.CreatedAtUtc.Should().Be(TestDates.SourceCreatedAtUtc);
        }

        [Fact]
        public void Archive_WhenSourceIsActive_ShouldRaiseArchivedEvent()
        {
            // Arrange
            Source source = new SourceBuilder()
                .BuildWithoutPendingEvents();

            // Act
            source.Archive(TestDates.SourceArchivedAtUtc)
                .ShouldSucceed();

            // Assert
            SourceArchivedDomainEvent domainEvent = source.ShouldHaveSingleDomainEvent<SourceArchivedDomainEvent>();

            domainEvent.SourceId.Should().Be(TestIds.DefaultSourceId);

            domainEvent.ArchivedAtUtc.Should().Be(TestDates.SourceArchivedAtUtc);
        }

        [Fact]
        public void Archive_WhenSourceIsAlreadyArchived_ShouldReturnFailure()
        {
            // Arrange
            Source source = new SourceBuilder().BuildWithoutPendingEvents();

            source.Archive(TestDates.SourceArchivedAtUtc).ShouldSucceed();

            source.DequeueDomainEvents();

            DateTimeOffset? originalArchivedAtUtc = source.ArchivedAtUtc;

            DateTimeOffset originalUpdatedAtUtc = source.UpdatedAtUtc;

            // Act
            DomainResult result = source.Archive(TestDates.LaterUtc);

            // Assert
            result.ShouldFailWith(SourceErrors.AlreadyArchived);

            source.Status.Should().Be(SourceStatusType.Archived);

            source.ArchivedAtUtc.Should().Be(originalArchivedAtUtc);

            source.UpdatedAtUtc.Should().Be(originalUpdatedAtUtc);

            source.ShouldHaveNoDomainEvents();
        }
    }
}