using Espada.Domain.Errors;

namespace Espada.Tests.Domain.Aggregates
{
    public sealed class ChunkBatchTests
    {
        [Fact]
        public void Request_WithValidValues_ShouldCreateRequestedBatch()
        {
            DomainResult<ChunkingVersion> versionResult = ChunkingVersion.Create("fixed-size-v1");
            DomainResult<ChunkBatch> result = ChunkBatch.Request(
                ChunkBatchId.New(),
                TestIds.DefaultWorkspaceId,
                TestIds.DefaultArtifactId,
                TestIds.DefaultArtifactRevisionId,
                ChunkingStrategyType.FixedSize,
                versionResult.Value,
                DateTimeOffset.UtcNow);

            result.IsFailure.Should().BeFalse();
            result.Value.Status.Should().Be(ChunkBatchStatusType.Requested);
        }

        [Fact]
        public void Complete_AfterStart_ShouldSucceed()
        {
            DomainResult<ChunkingVersion> versionResult = ChunkingVersion.Create("fixed-size-v1");
            ChunkBatch batch = ChunkBatch.Request(
                ChunkBatchId.New(),
                TestIds.DefaultWorkspaceId,
                TestIds.DefaultArtifactId,
                TestIds.DefaultArtifactRevisionId,
                ChunkingStrategyType.FixedSize,
                versionResult.Value,
                DateTimeOffset.UtcNow).Value;

            batch.Start(DateTimeOffset.UtcNow).IsFailure.Should().BeFalse();
            batch.Complete(2, DateTimeOffset.UtcNow).IsFailure.Should().BeFalse();
            batch.Status.Should().Be(ChunkBatchStatusType.Succeeded);
            batch.ChunkCount.Should().Be(2);
        }

        [Fact]
        public void Complete_WithoutStart_ShouldFail()
        {
            DomainResult<ChunkingVersion> versionResult = ChunkingVersion.Create("fixed-size-v1");
            ChunkBatch batch = ChunkBatch.Request(
                ChunkBatchId.New(),
                TestIds.DefaultWorkspaceId,
                TestIds.DefaultArtifactId,
                TestIds.DefaultArtifactRevisionId,
                ChunkingStrategyType.FixedSize,
                versionResult.Value,
                DateTimeOffset.UtcNow).Value;

            DomainResult result = batch.Complete(1, DateTimeOffset.UtcNow);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ChunkBatchErrors.CannotComplete);
        }
    }
}