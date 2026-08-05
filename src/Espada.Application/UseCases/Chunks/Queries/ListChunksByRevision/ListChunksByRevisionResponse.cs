namespace Espada.Application.UseCases.Chunks.Queries.ListChunksByRevision
{
    public sealed record ListChunksByRevisionResponse(IReadOnlyList<ChunkListItemResponse> Items);
}