using Espada.Domain.Errors;
using Espada.Tests.Domain.TestData.Builders;

namespace Espada.Tests.Domain.Aggregates;

public sealed class ImportJobTests
{
    [Fact]
    public void Request_WithValidArguments_ShouldCreateRequestedImportJob()
    {
        // Act
        ImportJob importJob = new ImportJobBuilder().BuildRequested();

        // Assert
        importJob.Id.Should().Be(TestIds.DefaultImportJobId);
        importJob.SourceId.Should().Be(TestIds.DefaultSourceId);
        importJob.WorkspaceId.Should().Be(TestIds.DefaultWorkspaceId);
        importJob.Status.Should().Be(ImportStatusType.Requested);
        importJob.RequestedAtUtc.Should().Be(TestDates.ImportRequestedAtUtc);
        importJob.StartedAtUtc.Should().BeNull();
        importJob.CompletedAtUtc.Should().BeNull();
        importJob.ArtifactId.Should().BeNull();
        importJob.ArtifactRevisionId.Should().BeNull();
        importJob.Failure.Should().BeNull();
    }

    [Fact]
    public void Request_WithValidArguments_ShouldRaiseRequestedEvent()
    {
        // Act
        ImportJob importJob = new ImportJobBuilder().BuildRequested();

        // Assert
        ImportJobRequestedDomainEvent domainEvent = importJob.ShouldHaveSingleDomainEvent<ImportJobRequestedDomainEvent>();

        domainEvent.ImportJobId.Should().Be(TestIds.DefaultImportJobId);
        domainEvent.SourceId.Should().Be(TestIds.DefaultSourceId);
        domainEvent.WorkspaceId.Should().Be(TestIds.DefaultWorkspaceId);
        domainEvent.RequestedAtUtc.Should().Be(TestDates.ImportRequestedAtUtc);
    }

    [Fact]
    public void Start_WhenRequested_ShouldMoveImportJobToRunning()
    {
        // Arrange
        ImportJob importJob = new ImportJobBuilder().BuildRequestedWithoutPendingEvents();

        // Act
        DomainResult result = importJob.Start(TestDates.ImportStartedAtUtc);

        // Assert
        result.ShouldSucceed();

        importJob.Status.Should().Be(ImportStatusType.Running);

        importJob.StartedAtUtc.Should().Be(TestDates.ImportStartedAtUtc);

        importJob.CompletedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Start_WhenRequested_ShouldRaiseStartedEvent()
    {
        // Arrange
        ImportJob importJob = new ImportJobBuilder().BuildRequestedWithoutPendingEvents();

        // Act
        importJob.Start(TestDates.ImportStartedAtUtc).ShouldSucceed();

        // Assert
        ImportJobStartedDomainEvent domainEvent = importJob.ShouldHaveSingleDomainEvent<ImportJobStartedDomainEvent>();

        domainEvent.ImportJobId.Should().Be(TestIds.DefaultImportJobId);

        domainEvent.StartedAtUtc.Should().Be(TestDates.ImportStartedAtUtc);
    }

    [Fact]
    public void Start_WhenAlreadyRunning_ShouldReturnFailure()
    {
        // Arrange
        ImportJob importJob = new ImportJobBuilder().BuildRunningWithoutPendingEvents();

        DateTimeOffset? originalStartedAtUtc = importJob.StartedAtUtc;

        // Act
        DomainResult result = importJob.Start(TestDates.LaterUtc);

        // Assert
        result.ShouldFailWith(ImportJobErrors.CannotStart);

        importJob.Status.Should().Be(ImportStatusType.Running);
        importJob.StartedAtUtc.Should().Be(originalStartedAtUtc);

        importJob.ShouldHaveNoDomainEvents();
    }

    [Fact]
    public void Complete_WhenRunning_ShouldMarkImportAsSucceeded()
    {
        // Arrange
        ImportJob importJob = new ImportJobBuilder().BuildRunningWithoutPendingEvents();

        // Act
        DomainResult result = importJob.Complete(TestIds.DefaultArtifactId, TestIds.FirstRevisionId, TestDates.ImportCompletedAtUtc);

        // Assert
        result.ShouldSucceed();

        importJob.Status.Should().Be(ImportStatusType.Succeeded);
        importJob.ArtifactId.Should().Be(TestIds.DefaultArtifactId);
        importJob.ArtifactRevisionId.Should().Be(TestIds.FirstRevisionId);
        importJob.CompletedAtUtc.Should().Be(TestDates.ImportCompletedAtUtc);
        importJob.Failure.Should().BeNull();
    }

    [Fact]
    public void Complete_WhenRunning_ShouldRaiseCompletedEvent()
    {
        // Arrange
        ImportJob importJob = new ImportJobBuilder()
            .BuildRunningWithoutPendingEvents();

        // Act
        importJob.Complete(TestIds.DefaultArtifactId, TestIds.FirstRevisionId, TestDates.ImportCompletedAtUtc).ShouldSucceed();

        // Assert
        ImportJobCompletedDomainEvent domainEvent = importJob.ShouldHaveSingleDomainEvent<ImportJobCompletedDomainEvent>();

        domainEvent.ImportJobId.Should().Be(TestIds.DefaultImportJobId);
        domainEvent.SourceId.Should().Be(TestIds.DefaultSourceId);
        domainEvent.ArtifactId.Should().Be(TestIds.DefaultArtifactId);
        domainEvent.ArtifactRevisionId.Should().Be(TestIds.FirstRevisionId);
        domainEvent.CompletedAtUtc.Should().Be(TestDates.ImportCompletedAtUtc);
    }

    [Fact]
    public void Complete_WhenRequested_ShouldReturnFailure()
    {
        // Arrange
        ImportJob importJob = new ImportJobBuilder().BuildRequestedWithoutPendingEvents();

        // Act
        DomainResult result = importJob.Complete(TestIds.DefaultArtifactId, TestIds.FirstRevisionId, TestDates.ImportCompletedAtUtc);

        // Assert
        result.ShouldFailWith(ImportJobErrors.CannotComplete);

        importJob.Status.Should().Be(ImportStatusType.Requested);

        importJob.ArtifactId.Should().BeNull();
        importJob.ArtifactRevisionId.Should().BeNull();
        importJob.CompletedAtUtc.Should().BeNull();

        importJob.ShouldHaveNoDomainEvents();
    }

    [Fact]
    public void Fail_WhenRunning_ShouldMarkImportAsFailed()
    {
        // Arrange
        ImportJob importJob = new ImportJobBuilder().BuildRunningWithoutPendingEvents();

        ImportFailure failure = ImportFailure.Create("source.read_failed", "The source could not be read.").ShouldSucceed();

        // Act
        DomainResult result = importJob.Fail(failure, TestDates.ImportCompletedAtUtc);

        // Assert
        result.ShouldSucceed();

        importJob.Status.Should().Be(ImportStatusType.Failed);
        importJob.Failure.Should().Be(failure);
        importJob.CompletedAtUtc.Should().Be(TestDates.ImportCompletedAtUtc);
        importJob.ArtifactId.Should().BeNull();
        importJob.ArtifactRevisionId.Should().BeNull();
    }

    [Fact]
    public void Fail_WhenRunning_ShouldRaiseFailedEvent()
    {
        // Arrange
        ImportJob importJob = new ImportJobBuilder().BuildRunningWithoutPendingEvents();

        ImportFailure failure = ImportFailure.Create("source.read_failed", "The source could not be read.").ShouldSucceed();

        // Act
        importJob.Fail(failure, TestDates.ImportCompletedAtUtc).ShouldSucceed();

        // Assert
        ImportJobFailedDomainEvent domainEvent = importJob.ShouldHaveSingleDomainEvent<ImportJobFailedDomainEvent>();

        domainEvent.ImportJobId.Should().Be(TestIds.DefaultImportJobId);
        domainEvent.FailureCode.Should().Be("source.read_failed");
        domainEvent.FailureReason.Should().Be("The source could not be read.");
        domainEvent.FailedAtUtc.Should().Be(TestDates.ImportCompletedAtUtc);
    }

    [Fact]
    public void Fail_WhenRequested_ShouldReturnFailure()
    {
        // Arrange
        ImportJob importJob = new ImportJobBuilder().BuildRequestedWithoutPendingEvents();

        ImportFailure failure = ImportFailure.Create("source.read_failed", "The source could not be read.").ShouldSucceed();

        // Act
        DomainResult result = importJob.Fail(failure, TestDates.ImportCompletedAtUtc);

        // Assert
        result.ShouldFailWith(ImportJobErrors.CannotFail);

        importJob.Status.Should().Be(ImportStatusType.Requested);
        importJob.Failure.Should().BeNull();
        importJob.CompletedAtUtc.Should().BeNull();

        importJob.ShouldHaveNoDomainEvents();
    }

    [Fact]
    public void Cancel_WhenRequested_ShouldCancelImportJob()
    {
        // Arrange
        ImportJob importJob = new ImportJobBuilder().BuildRequestedWithoutPendingEvents();

        // Act
        DomainResult result = importJob.Cancel(TestDates.ImportCompletedAtUtc);

        // Assert
        result.ShouldSucceed();

        importJob.Status.Should().Be(ImportStatusType.Cancelled);
        importJob.CompletedAtUtc.Should().Be(TestDates.ImportCompletedAtUtc);
        importJob.StartedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Cancel_WhenRunning_ShouldCancelImportJob()
    {
        // Arrange
        ImportJob importJob = new ImportJobBuilder().BuildRunningWithoutPendingEvents();

        // Act
        DomainResult result = importJob.Cancel(TestDates.ImportCompletedAtUtc);

        // Assert
        result.ShouldSucceed();

        importJob.Status.Should().Be(ImportStatusType.Cancelled);
        importJob.StartedAtUtc.Should().Be(TestDates.ImportStartedAtUtc);
        importJob.CompletedAtUtc.Should().Be(TestDates.ImportCompletedAtUtc);
    }

    [Fact]
    public void Cancel_WhenAllowed_ShouldRaiseCancelledEvent()
    {
        // Arrange
        ImportJob importJob = new ImportJobBuilder().BuildRequestedWithoutPendingEvents();

        // Act
        importJob.Cancel(TestDates.ImportCompletedAtUtc).ShouldSucceed();

        // Assert
        ImportJobCancelledDomainEvent domainEvent = importJob.ShouldHaveSingleDomainEvent<ImportJobCancelledDomainEvent>();

        domainEvent.ImportJobId.Should().Be(TestIds.DefaultImportJobId);

        domainEvent.CancelledAtUtc.Should().Be(TestDates.ImportCompletedAtUtc);
    }

    [Fact]
    public void CompletedImportJob_ShouldRejectFurtherTransitions()
    {
        // Arrange
        ImportJob importJob = CreateSucceededImportWithoutPendingEvents();

        DateTimeOffset? originalCompletedAtUtc = importJob.CompletedAtUtc;

        // Act
        DomainResult startResult = importJob.Start(TestDates.LaterUtc);

        DomainResult completeResult = importJob.Complete(TestIds.DefaultArtifactId, TestIds.FirstRevisionId, TestDates.LaterUtc);

        DomainResult failResult = importJob.Fail(CreateFailure(), TestDates.LaterUtc);

        DomainResult cancelResult = importJob.Cancel(TestDates.LaterUtc);

        // Assert
        startResult.ShouldFailWith(ImportJobErrors.CannotStart);

        completeResult.ShouldFailWith(ImportJobErrors.CannotComplete);

        failResult.ShouldFailWith(ImportJobErrors.CannotFail);

        cancelResult.ShouldFailWith(ImportJobErrors.CannotCancel);

        importJob.Status.Should().Be(ImportStatusType.Succeeded);
        importJob.CompletedAtUtc.Should().Be(originalCompletedAtUtc);
        importJob.ArtifactId.Should().Be(TestIds.DefaultArtifactId);
        importJob.ArtifactRevisionId.Should().Be(TestIds.FirstRevisionId);

        importJob.ShouldHaveNoDomainEvents();
    }

    [Fact]
    public void FailedImportJob_ShouldRejectFurtherTransitions()
    {
        // Arrange
        ImportJob importJob = new ImportJobBuilder().BuildRunningWithoutPendingEvents();

        ImportFailure originalFailure = CreateFailure();

        importJob.Fail(originalFailure, TestDates.ImportCompletedAtUtc).ShouldSucceed();

        importJob.DequeueDomainEvents();

        // Act
        DomainResult startResult = importJob.Start(TestDates.LaterUtc);

        DomainResult completeResult = importJob.Complete(TestIds.DefaultArtifactId, TestIds.FirstRevisionId, TestDates.LaterUtc);

        DomainResult failResult = importJob.Fail(ImportFailure.Create("another.failure", "Another failure.").ShouldSucceed(), TestDates.LaterUtc);

        DomainResult cancelResult = importJob.Cancel(TestDates.LaterUtc);

        // Assert
        startResult.ShouldFailWith(ImportJobErrors.CannotStart);

        completeResult.ShouldFailWith(ImportJobErrors.CannotComplete);

        failResult.ShouldFailWith(ImportJobErrors.CannotFail);

        cancelResult.ShouldFailWith(ImportJobErrors.CannotCancel);

        importJob.Status.Should().Be(ImportStatusType.Failed);

        importJob.Failure.Should().Be(originalFailure);

        importJob.CompletedAtUtc.Should().Be(TestDates.ImportCompletedAtUtc);

        importJob.ShouldHaveNoDomainEvents();
    }

    [Fact]
    public void CancelledImportJob_ShouldRejectFurtherTransitions()
    {
        // Arrange
        ImportJob importJob = new ImportJobBuilder().BuildRequestedWithoutPendingEvents();

        importJob.Cancel(TestDates.ImportCompletedAtUtc).ShouldSucceed();

        importJob.DequeueDomainEvents();

        // Act
        DomainResult startResult = importJob.Start(TestDates.LaterUtc);

        DomainResult completeResult = importJob.Complete(TestIds.DefaultArtifactId, TestIds.FirstRevisionId, TestDates.LaterUtc);

        DomainResult failResult = importJob.Fail(CreateFailure(), TestDates.LaterUtc);

        DomainResult cancelResult = importJob.Cancel(TestDates.LaterUtc);

        // Assert
        startResult.ShouldFailWith(ImportJobErrors.CannotStart);

        completeResult.ShouldFailWith(ImportJobErrors.CannotComplete);

        failResult.ShouldFailWith(ImportJobErrors.CannotFail);

        cancelResult.ShouldFailWith(ImportJobErrors.CannotCancel);

        importJob.Status.Should().Be(ImportStatusType.Cancelled);

        importJob.CompletedAtUtc.Should().Be(TestDates.ImportCompletedAtUtc);

        importJob.ShouldHaveNoDomainEvents();
    }

    private static ImportJob CreateSucceededImportWithoutPendingEvents()
    {
        ImportJob importJob = new ImportJobBuilder()
            .BuildRunningWithoutPendingEvents();

        importJob.Complete(TestIds.DefaultArtifactId, TestIds.FirstRevisionId, TestDates.ImportCompletedAtUtc)
            .ShouldSucceed();

        importJob.DequeueDomainEvents();

        return importJob;
    }

    private static ImportFailure CreateFailure()
    {
        return ImportFailure.Create(
                "source.read_failed",
                "The source could not be read.")
            .ShouldSucceed();
    }
}