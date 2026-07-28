using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class ChunkBatchRepository(EspadaDbContext dbContext) : IChunkBatchRepository
    {
        public async Task AddAsync(ChunkBatch chunkBatch, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(chunkBatch);

            await dbContext.ChunkBatches.AddAsync(chunkBatch, cancellationToken);
        }

        public async Task<ChunkBatch?> GetByIdAsync(ChunkBatchId chunkBatchId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(chunkBatchId);

            return await dbContext.ChunkBatches.FindAsync([chunkBatchId], cancellationToken);
        }
    }
}