using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Chunks.Queries.GetChunkById
{
    public sealed record GetChunkByIdQuery(Guid WorkspaceId, Guid ChunkId) : IQuery<GetChunkByIdResponse>;
}