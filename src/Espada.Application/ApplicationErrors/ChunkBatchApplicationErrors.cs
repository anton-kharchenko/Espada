using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors
{
    public static class ChunkBatchApplicationErrors
    {
        public static readonly DomainError InvalidId = new("ChunkBatch.Id.Invalid", "Chunk batch ID cannot be empty.");

        public static DomainError UnsupportedStrategy(int strategyId)
        {
            return new DomainError("ChunkBatch.Strategy.Unsupported",
                $"Chunking strategy with ID '{strategyId}' is not supported.");
        }

        public static DomainError NotFound(Guid chunkBatchId)
        {
            return new DomainError("ChunkBatch.NotFound", $"Chunk batch with ID '{chunkBatchId:D}' was not found.");
        }

        public static DomainError NotFoundInWorkspace(Guid chunkBatchId, Guid workspaceId)
        {
            return new DomainError("ChunkBatch.NotFoundInWorkspace",
                $"Chunk batch with ID '{chunkBatchId:D}' was not found in workspace '{workspaceId:D}'.");
        }
    }
}