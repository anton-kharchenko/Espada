namespace Espada.Application.UseCases.Chunks.Commands.CreateChunkBatch
{
    public sealed record CreateChunkBatchResponse(
        Guid ChunkBatchId,
        Guid ArtifactRevisionId,
        int StrategyId,
        string StrategyName,
        string StrategyVersion,
        int StatusId,
        string StatusName,
        DateTimeOffset RequestedAtUtc);
}