using Espada.Application.Contracts.Persistence;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Espada.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class EmbeddingVectorStore(EspadaDbContext dbContext) : IEmbeddingVectorStore
    {
        public async Task AddAsync(ChunkEmbeddingId chunkEmbeddingId, IReadOnlyList<float> vector, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(chunkEmbeddingId);
            ArgumentNullException.ThrowIfNull(vector);

            await dbContext.EmbeddingVectors.AddAsync(new EmbeddingVectorRecord(chunkEmbeddingId, vector), cancellationToken);
        }

        public async Task<IReadOnlyList<float>?> GetByIdAsync(ChunkEmbeddingId chunkEmbeddingId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(chunkEmbeddingId);

            return await dbContext.EmbeddingVectors
                .AsNoTracking()
                .Where(record => record.ChunkEmbeddingId == chunkEmbeddingId)
                .Select(record => record.Vector)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}