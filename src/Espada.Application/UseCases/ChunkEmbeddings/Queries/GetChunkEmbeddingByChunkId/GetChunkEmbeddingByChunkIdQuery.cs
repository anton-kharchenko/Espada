using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.ChunkEmbeddings.Queries.GetChunkEmbeddingByChunkId
{
    public sealed record GetChunkEmbeddingByChunkIdQuery(
        Guid WorkspaceId,
        Guid ChunkId,
        string ModelIdentifier,
        string ModelVersion) : IQuery<GetChunkEmbeddingByChunkIdResponse>;
}