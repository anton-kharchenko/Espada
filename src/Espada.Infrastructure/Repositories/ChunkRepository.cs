using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class ChunkRepository(EspadaDbContext dbContext) : IChunkRepository
    {
        public async Task AddRangeAsync(IReadOnlyCollection<Chunk> chunks, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(chunks);

            if (chunks.Count == 0)
            {
                return;
            }

            await dbContext.Chunks.AddRangeAsync(chunks, cancellationToken);
        }

        public async Task<Chunk?> GetByIdAsync(ChunkId chunkId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(chunkId);

            return await dbContext.Chunks
                .AsNoTracking()
                .SingleOrDefaultAsync(chunk => chunk.Id == chunkId, cancellationToken);
        }

        public async Task<IReadOnlyList<Chunk>> ListByArtifactRevisionIdAsync(ArtifactRevisionId artifactRevisionId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(artifactRevisionId);

            return await dbContext.Chunks
                .AsNoTracking()
                .Where(chunk => chunk.ArtifactRevisionId == artifactRevisionId)
                .OrderBy(chunk => chunk.Number)
                .ToListAsync(cancellationToken);
        }
    }
}