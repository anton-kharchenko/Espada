using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories;

internal sealed class ChunkEmbeddingRepository(EspadaDbContext dbContext) : IChunkEmbeddingRepository
{
    public async Task AddAsync(ChunkEmbedding chunkEmbedding, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chunkEmbedding);

        await dbContext.ChunkEmbeddings.AddAsync(chunkEmbedding, cancellationToken);
    }

    public async Task<ChunkEmbedding?> GetByChunkIdAsync(ChunkId chunkId, EmbeddingModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chunkId);
        ArgumentNullException.ThrowIfNull(model);

        return await dbContext.ChunkEmbeddings
            .AsNoTracking()
            .SingleOrDefaultAsync(
                embedding =>
                    embedding.ChunkId == chunkId &&
                    EF.Property<string>(embedding, Db.Constants.DbPropertyConstants.ChunkEmbeddingModelIdentifier) == model.Identifier &&
                    EF.Property<string>(embedding, Db.Constants.DbPropertyConstants.ChunkEmbeddingModelVersion) == model.Version,
                cancellationToken);
    }

    public async Task<IReadOnlyList<int>> ListDimensionsAsync(WorkspaceId workspaceId, EmbeddingModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspaceId);
        ArgumentNullException.ThrowIfNull(model);

        return await dbContext.ChunkEmbeddings
            .AsNoTracking()
            .Where(embedding =>
                embedding.WorkspaceId == workspaceId &&
                EF.Property<string>(embedding, Db.Constants.DbPropertyConstants.ChunkEmbeddingModelIdentifier) == model.Identifier &&
                EF.Property<string>(embedding, Db.Constants.DbPropertyConstants.ChunkEmbeddingModelVersion) == model.Version)
            .Select(embedding => embedding.Dimensions.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}