namespace Espada.Application.UseCases.Chunks.Commands.CreateChunks;

public sealed record CreateChunkItem(int Number, string Content, int? SourceStart, int? SourceLength);