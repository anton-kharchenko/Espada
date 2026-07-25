using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence;

public interface IChunkEmbeddingRepository
{
    Task AddAsync(ChunkEmbedding chunkEmbedding, CancellationToken cancellationToken = default);
    Task<ChunkEmbedding?> GetByChunkIdAsync(ChunkId chunkId, CancellationToken cancellationToken = default);
}