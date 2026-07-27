using Espada.Domain.Errors;
using Espada.Tests.Domain.TestData.Builders;

namespace Espada.Tests.Domain.Aggregates;

public sealed class ImportPipelineStateTests
{
    [Fact]
    public void Request_ShouldCaptureIdempotencyAndStartStage()
    {
        ImportJob importJob = ImportJob.Request(TestIds.DefaultImportJobId, TestIds.DefaultSourceId, TestIds.DefaultWorkspaceId, TestDates.ImportRequestedAtUtc, "request-123", "sha256:payload", """{"chunking":{"strategy":"Recursive"}}""").ShouldSucceed();
        importJob.IdempotencyKey.Should().Be("request-123");
        importJob.RequestFingerprint.Should().Be("sha256:payload");
        importJob.OptionsJson.Should().Be("""{"chunking":{"strategy":"Recursive"}}""");
        importJob.Stage.Should().Be(ImportPipelineStageType.Start);
    }

    [Fact]
    public void CompleteStage_WhenStartSucceeded_ShouldScheduleReadExactlyOnce()
    {
        ImportJob importJob = new ImportJobBuilder().BuildRequestedWithoutPendingEvents();

        importJob.CompleteStage(ImportPipelineStageType.Start, TestDates.ImportStartedAtUtc).ShouldSucceed();

        importJob.Status.Should().Be(ImportStatusType.Running);
        importJob.Stage.Should().Be(ImportPipelineStageType.Read);
        ImportStageScheduledDomainEvent domainEvent = importJob.ShouldHaveSingleDomainEvent<ImportStageScheduledDomainEvent>();
        domainEvent.Stage.Should().Be(ImportPipelineStageType.Read);

        importJob.DequeueDomainEvents();
        importJob.CompleteStage(ImportPipelineStageType.Start, TestDates.LaterUtc).ShouldSucceed();
        importJob.Stage.Should().Be(ImportPipelineStageType.Read);
        importJob.ShouldHaveNoDomainEvents();
    }

    [Fact]
    public void CompleteStage_WhenStageIsSkipped_ShouldFailWithoutChangingState()
    {
        ImportJob importJob = new ImportJobBuilder().BuildRequestedWithoutPendingEvents();

        DomainResult result = importJob.CompleteStage(ImportPipelineStageType.Parse, TestDates.ImportStartedAtUtc);

        result.ShouldFailWith(ImportJobErrors.CannotAdvanceStage);
        importJob.Status.Should().Be(ImportStatusType.Requested);
        importJob.Stage.Should().Be(ImportPipelineStageType.Start);
        importJob.ShouldHaveNoDomainEvents();
    }

    [Fact]
    public void PipelineReferences_ShouldBeRecordedIdempotently()
    {
        ImportJob importJob = new ImportJobBuilder().BuildRunningWithoutPendingEvents();
        ChunkBatchId chunkBatchId = ChunkBatchId.Create(Guid.NewGuid());

        importJob.RecordRawSnapshot("sha256:raw").ShouldSucceed();
        importJob.RecordRawSnapshot("sha256:raw").ShouldSucceed();
        importJob.RecordParsedSnapshot("sha256:parsed").ShouldSucceed();
        importJob.RecordMaterializedArtifact(TestIds.DefaultArtifactId, TestIds.FirstRevisionId).ShouldSucceed();
        importJob.RecordChunkBatch(chunkBatchId).ShouldSucceed();

        importJob.RawBlobHash.Should().Be("sha256:raw");
        importJob.ParsedBlobHash.Should().Be("sha256:parsed");
        importJob.ArtifactId.Should().Be(TestIds.DefaultArtifactId);
        importJob.ArtifactRevisionId.Should().Be(TestIds.FirstRevisionId);
        importJob.ChunkBatchId.Should().Be(chunkBatchId);
    }
}