using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class ChunkRepositorySpy : IChunkRepository
    {
        public IReadOnlyCollection<Chunk> AddedChunks { get; private set; } = Array.Empty<Chunk>();
        public Chunk? ChunkToReturn { get; set; }
        public IReadOnlyList<Chunk> ChunksToReturn { get; set; } = Array.Empty<Chunk>();
        public int AddRangeCallCount { get; private set; }
        public int GetByIdCallCount { get; private set; }
        public int ListCallCount { get; private set; }
        public CancellationToken AddRangeCancellationToken { get; private set; }
        public CancellationToken GetByIdCancellationToken { get; private set; }
        public CancellationToken ListCancellationToken { get; private set; }

        public Task AddRangeAsync(IReadOnlyCollection<Chunk> chunks, CancellationToken cancellationToken = default)
        {
            AddedChunks = chunks;
            AddRangeCallCount++;
            AddRangeCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }

        public Task<Chunk?> GetByIdAsync(ChunkId chunkId, CancellationToken cancellationToken = default)
        {
            GetByIdCallCount++;
            GetByIdCancellationToken = cancellationToken;
            return Task.FromResult(ChunkToReturn);
        }

        public Task<IReadOnlyList<Chunk>> ListByArtifactRevisionIdAsync(ArtifactRevisionId artifactRevisionId,
            CancellationToken cancellationToken = default)
        {
            ListCallCount++;
            ListCancellationToken = cancellationToken;
            return Task.FromResult(ChunksToReturn);
        }
    }
}