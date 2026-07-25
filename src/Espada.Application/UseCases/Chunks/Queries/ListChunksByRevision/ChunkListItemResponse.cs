namespace Espada.Application.UseCases.Chunks.Queries.ListChunksByRevision;

public sealed record ChunkListItemResponse(
    Guid Id,
    Guid BatchId,
    int Number,
    string ContentHash,
    int SizeInBytes,
    int CharacterCount,
    int? SourceStart,
    int? SourceLength,
    DateTimeOffset CreatedAtUtc);