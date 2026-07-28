using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class EmbeddingVectorStoreSpy : IEmbeddingVectorStore
    {
        public CancellationToken UpsertCancellationToken { get; private set; }

        public Task UpsertAsync(
            ChunkEmbeddingId chunkEmbeddingId,
            IReadOnlyList<float> vector,
            CancellationToken cancellationToken = default)
        {
            UpsertCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<float>?> GetByIdAsync(
            ChunkEmbeddingId chunkEmbeddingId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<float>?>(null);
        }

        public Task DeleteAsync(
            ChunkEmbeddingId chunkEmbeddingId,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<EmbeddingVectorSearchHit>> SearchNearestAsync(
            EmbeddingVectorSearch search,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<EmbeddingVectorSearchHit>>([]);
        }

        public Task DeleteByWorkspaceAsync(
            WorkspaceId workspaceId,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}