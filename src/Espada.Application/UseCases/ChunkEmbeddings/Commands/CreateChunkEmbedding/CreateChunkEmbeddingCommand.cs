using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.ChunkEmbeddings.Commands.CreateChunkEmbedding
{
    public sealed record CreateChunkEmbeddingCommand(
        Guid WorkspaceId,
        Guid ChunkId,
        string ModelIdentifier,
        string ModelVersion,
        IReadOnlyList<float> Vector) : ICommand<CreateChunkEmbeddingResponse>;
}