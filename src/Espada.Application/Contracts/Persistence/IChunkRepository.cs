using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IChunkRepository
    {
        Task AddRangeAsync(IReadOnlyCollection<Chunk> chunks, CancellationToken cancellationToken = default);
        Task<Chunk?> GetByIdAsync(ChunkId chunkId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Chunk>> ListByArtifactRevisionIdAsync(ArtifactRevisionId artifactRevisionId,
            CancellationToken cancellationToken = default);
    }
}