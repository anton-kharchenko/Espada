namespace Espada.Application.UseCases.ChunkEmbeddings.Queries.GetChunkEmbeddingByChunkId;

public sealed record GetChunkEmbeddingByChunkIdResponse(
    Guid Id,
    Guid ChunkId,
    string ChunkContentHash,
    string ModelIdentifier,
    string ModelVersion,
    int Dimensions,
    IReadOnlyList<float> Vector,
    DateTimeOffset CreatedAtUtc);