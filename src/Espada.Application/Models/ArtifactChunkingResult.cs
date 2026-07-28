using Espada.Domain.Aggregates;

namespace Espada.Application.Models
{
    internal sealed record ArtifactChunkingResult(
        ChunkBatch Batch,
        IReadOnlyList<Chunk> Chunks);
}