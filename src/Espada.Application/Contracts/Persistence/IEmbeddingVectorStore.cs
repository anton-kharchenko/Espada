using Espada.Application.Models;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence;

public interface IEmbeddingVectorStore
{
    Task UpsertAsync(ChunkEmbeddingId chunkEmbeddingId, IReadOnlyList<float> vector, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<float>?> GetByIdAsync(ChunkEmbeddingId chunkEmbeddingId, CancellationToken cancellationToken = default);
    Task DeleteAsync(ChunkEmbeddingId chunkEmbeddingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmbeddingVectorSearchHit>> SearchNearestAsync(EmbeddingVectorSearch search, CancellationToken cancellationToken = default);
    Task DeleteByWorkspaceAsync(WorkspaceId workspaceId, CancellationToken cancellationToken = default);
}