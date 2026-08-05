using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class ChunkEmbeddingRepositorySpy : IChunkEmbeddingRepository
    {
        public ChunkEmbedding? AddedEmbedding { get; private set; }
        public CancellationToken AddCancellationToken { get; private set; }

        public Task AddAsync(
            ChunkEmbedding chunkEmbedding,
            CancellationToken cancellationToken = default)
        {
            AddedEmbedding = chunkEmbedding;
            AddCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }

        public Task<ChunkEmbedding?> GetByChunkIdAsync(
            ChunkId chunkId,
            EmbeddingModel model,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ChunkEmbedding?>(null);
        }

        public Task<IReadOnlyList<int>> ListDimensionsAsync(
            WorkspaceId workspaceId,
            EmbeddingModel model,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<int>>([]);
        }
    }
}