namespace Espada.Application.UseCases.Chunks.Commands.CreateChunks
{
    public sealed record CreateChunksResponse(
        Guid ChunkBatchId,
        int ChunkCount,
        DateTimeOffset CompletedAtUtc,
        IReadOnlyList<CreatedChunkResponse> Items);
}