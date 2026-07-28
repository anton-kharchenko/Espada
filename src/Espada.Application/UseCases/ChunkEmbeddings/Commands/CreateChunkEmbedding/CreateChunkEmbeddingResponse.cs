namespace Espada.Application.UseCases.ChunkEmbeddings.Commands.CreateChunkEmbedding
{
    public sealed record CreateChunkEmbeddingResponse(
        Guid ChunkEmbeddingId,
        Guid ChunkId,
        string ChunkContentHash,
        string ModelIdentifier,
        string ModelVersion,
        int Dimensions,
        DateTimeOffset CreatedAtUtc);
}