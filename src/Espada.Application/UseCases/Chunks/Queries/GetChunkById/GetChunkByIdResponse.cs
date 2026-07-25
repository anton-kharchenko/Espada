namespace Espada.Application.UseCases.Chunks.Queries.GetChunkById;

public sealed record GetChunkByIdResponse(
    Guid Id,
    Guid BatchId,
    Guid WorkspaceId,
    Guid ArtifactId,
    Guid ArtifactRevisionId,
    int Number,
    string Content,
    string ContentHash,
    int SizeInBytes,
    int CharacterCount,
    int? SourceStart,
    int? SourceLength,
    int StrategyId,
    string StrategyName,
    string StrategyVersion,
    DateTimeOffset CreatedAtUtc);