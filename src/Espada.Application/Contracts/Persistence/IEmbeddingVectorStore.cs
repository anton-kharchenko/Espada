using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence;

public interface IEmbeddingVectorStore
{
    Task AddAsync(ChunkEmbeddingId chunkEmbeddingId, IReadOnlyList<float> vector, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<float>?> GetByIdAsync(ChunkEmbeddingId chunkEmbeddingId, CancellationToken cancellationToken = default);
}