using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Chunks.Commands.CreateChunks
{
    public sealed record CreateChunksCommand(Guid WorkspaceId, Guid ChunkBatchId, IReadOnlyList<CreateChunkItem> Items)
        : ICommand<CreateChunksResponse>;
}