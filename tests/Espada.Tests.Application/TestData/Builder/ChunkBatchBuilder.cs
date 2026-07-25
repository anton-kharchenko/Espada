using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.TestData.Builder;

internal sealed class ChunkBatchBuilder
{
    private ChunkBatchId _id = TestIds.DefaultChunkBatchId;
    private WorkspaceId _workspaceId = TestIds.DefaultWorkspaceId;
    private ArtifactId _artifactId = TestIds.DefaultArtifactId;
    private ArtifactRevisionId _artifactRevisionId = TestIds.DefaultArtifactRevisionId;
    private readonly ChunkingStrategyType _strategy = ChunkingStrategyType.FixedSize;
    private const string? StrategyVersion = TestValues.ChunkingStrategyVersion;
    private readonly DateTimeOffset _requestedAtUtc = TestDates.ChunkBatchRequestedAtUtc;

    public ChunkBatchBuilder WithId(ChunkBatchId id) { _id = id; return this; }
    public ChunkBatchBuilder InWorkspace(WorkspaceId workspaceId) { _workspaceId = workspaceId; return this; }
    public ChunkBatchBuilder ForArtifact(ArtifactId artifactId) { _artifactId = artifactId; return this; }
    public ChunkBatchBuilder ForRevision(ArtifactRevisionId artifactRevisionId) { _artifactRevisionId = artifactRevisionId; return this; }

    public ChunkBatch BuildWithoutPendingEvents()
    {
        DomainResult<ChunkingVersion> versionResult = ChunkingVersion.Create(StrategyVersion);

        if (versionResult.IsFailure)
        {
            throw new InvalidOperationException(versionResult.Error.Description);
        }

        DomainResult<ChunkBatch> result = ChunkBatch.Request(_id, _workspaceId, _artifactId, _artifactRevisionId, _strategy, versionResult.Value, _requestedAtUtc);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Description);
        }

        result.Value.DequeueDomainEvents();
        return result.Value;
    }
}