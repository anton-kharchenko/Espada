using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;
using Espada.Db.Constants;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Espada.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class EmbeddingVectorStore(EspadaDbContext dbContext) : IEmbeddingVectorStore
    {
        public async Task UpsertAsync(ChunkEmbeddingId chunkEmbeddingId, IReadOnlyList<float> vector,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(chunkEmbeddingId);
            ArgumentNullException.ThrowIfNull(vector);

            EmbeddingVectorRecord? existing =
                await dbContext.EmbeddingVectors.FindAsync([chunkEmbeddingId], cancellationToken);

            if (existing is null)
            {
                await dbContext.EmbeddingVectors.AddAsync(new EmbeddingVectorRecord(chunkEmbeddingId, vector),
                    cancellationToken);
                return;
            }

            existing.Replace(vector);
        }

        public async Task<IReadOnlyList<float>?> GetByIdAsync(ChunkEmbeddingId chunkEmbeddingId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(chunkEmbeddingId);

            return await dbContext.EmbeddingVectors
                .AsNoTracking()
                .Where(record => record.ChunkEmbeddingId == chunkEmbeddingId)
                .Select(record => record.Vector.ToArray())
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task DeleteAsync(ChunkEmbeddingId chunkEmbeddingId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(chunkEmbeddingId);

            await dbContext.EmbeddingVectors
                .Where(record => record.ChunkEmbeddingId == chunkEmbeddingId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EmbeddingVectorSearchHit>> SearchNearestAsync(EmbeddingVectorSearch search,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(search);

            Vector queryVector = new(search.QueryVector.ToArray());
            EmbeddingDimensions dimensions = EmbeddingDimensions.Create(search.QueryVector.Count).Value!;
            var candidates =
                from vector in dbContext.EmbeddingVectors.AsNoTracking()
                join embedding in dbContext.ChunkEmbeddings.AsNoTracking()
                    on vector.ChunkEmbeddingId equals embedding.Id
                where embedding.WorkspaceId == search.WorkspaceId
                      && EF.Property<string>(embedding, DbPropertyConstants.ChunkEmbeddingModelIdentifier) ==
                      search.Model.Identifier
                      && EF.Property<string>(embedding, DbPropertyConstants.ChunkEmbeddingModelVersion) ==
                      search.Model.Version
                      && embedding.Dimensions == dimensions
                select new
                {
                    embedding.Id, embedding.ChunkId, Similarity = 1 - vector.Vector.CosineDistance(queryVector)
                };

            if (search.MinimumSimilarity.HasValue)
            {
                double minimumSimilarity = search.MinimumSimilarity.Value;
                candidates = candidates.Where(candidate => candidate.Similarity >= minimumSimilarity);
            }

            return await candidates
                .OrderByDescending(candidate => candidate.Similarity)
                .ThenBy(candidate => candidate.Id)
                .Select(candidate =>
                    new EmbeddingVectorSearchHit(candidate.Id, candidate.ChunkId, candidate.Similarity))
                .Take(search.TopK)
                .ToListAsync(cancellationToken);
        }

        public async Task DeleteByWorkspaceAsync(WorkspaceId workspaceId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);

            IQueryable<ChunkEmbeddingId> embeddingIds = dbContext.ChunkEmbeddings
                .Where(embedding => embedding.WorkspaceId == workspaceId)
                .Select(embedding => embedding.Id);

            await dbContext.EmbeddingVectors
                .Where(record => embeddingIds.Contains(record.ChunkEmbeddingId))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}