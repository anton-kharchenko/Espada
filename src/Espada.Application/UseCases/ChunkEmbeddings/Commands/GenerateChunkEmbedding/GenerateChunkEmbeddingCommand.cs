using Espada.Application.Contracts.Messaging;
using Espada.Application.UseCases.ChunkEmbeddings.Commands.CreateChunkEmbedding;

namespace Espada.Application.UseCases.ChunkEmbeddings.Commands.GenerateChunkEmbedding;

public sealed record GenerateChunkEmbeddingCommand(
    Guid WorkspaceId,
    Guid ChunkId,
    string ModelIdentifier,
    string ModelVersion) : ICommand<CreateChunkEmbeddingResponse>;