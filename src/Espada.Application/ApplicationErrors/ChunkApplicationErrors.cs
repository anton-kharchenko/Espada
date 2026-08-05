using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors
{
    public static class ChunkApplicationErrors
    {
        public static readonly DomainError InvalidId = new("Chunk.Id.Invalid", "Chunk ID cannot be empty.");
        public static readonly DomainError ItemsEmpty = new("Chunk.Items.Empty", "At least one chunk is required.");

        public static readonly DomainError NumbersNotSequential = new("Chunk.Numbers.NotSequential",
            "Chunk numbers must be sequential and start at one.");

        public static readonly DomainError SourceSpanIncomplete = new("Chunk.SourceSpan.Incomplete",
            "Source span start and length must either both be provided or both be omitted.");

        public static DomainError NotFound(Guid chunkId)
        {
            return new DomainError("Chunk.NotFound", $"Chunk with ID '{chunkId:D}' was not found.");
        }

        public static DomainError NotFoundInWorkspace(Guid chunkId, Guid workspaceId)
        {
            return new DomainError("Chunk.NotFoundInWorkspace",
                $"Chunk with ID '{chunkId:D}' was not found in workspace '{workspaceId:D}'.");
        }
    }
}