using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IChunkBatchRepository
    {
        Task AddAsync(ChunkBatch chunkBatch, CancellationToken cancellationToken = default);
        Task<ChunkBatch?> GetByIdAsync(ChunkBatchId chunkBatchId, CancellationToken cancellationToken = default);
    }
}