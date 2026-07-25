using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class ChunkEmbeddingRepository(EspadaDbContext dbContext) : IChunkEmbeddingRepository
    {
        public async Task AddAsync(ChunkEmbedding chunkEmbedding, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(chunkEmbedding);

            await dbContext.ChunkEmbeddings.AddAsync(chunkEmbedding, cancellationToken);
        }

        public async Task<ChunkEmbedding?> GetByChunkIdAsync(ChunkId chunkId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(chunkId);

            return await dbContext.ChunkEmbeddings
                .AsNoTracking()
                .SingleOrDefaultAsync(embedding => embedding.ChunkId == chunkId, cancellationToken);
        }
    }
}