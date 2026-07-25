namespace Espada.Application.UseCases.Chunks.Commands.CreateChunks;

public sealed record CreatedChunkResponse(Guid Id, int Number, string ContentHash, int SizeInBytes, int CharacterCount);