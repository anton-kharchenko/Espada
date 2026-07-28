using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class ChunkBatchRepositorySpy : IChunkBatchRepository
    {
        public ChunkBatch? AddedChunkBatch { get; private set; }
        public ChunkBatch? ChunkBatchToReturn { get; set; }
        public int AddCallCount { get; private set; }
        public int GetByIdCallCount { get; private set; }
        public ChunkBatchId? ReceivedChunkBatchId { get; private set; }
        public CancellationToken AddCancellationToken { get; private set; }
        public CancellationToken GetByIdCancellationToken { get; private set; }

        public Task AddAsync(ChunkBatch chunkBatch, CancellationToken cancellationToken = default)
        {
            AddedChunkBatch = chunkBatch;
            AddCallCount++;
            AddCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }

        public Task<ChunkBatch?> GetByIdAsync(ChunkBatchId chunkBatchId, CancellationToken cancellationToken = default)
        {
            ReceivedChunkBatchId = chunkBatchId;
            GetByIdCallCount++;
            GetByIdCancellationToken = cancellationToken;
            return Task.FromResult(ChunkBatchToReturn);
        }
    }
}