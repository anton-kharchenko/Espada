using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Chunks.Commands.CreateChunkBatch;

public sealed record CreateChunkBatchCommand(
    Guid WorkspaceId,
    Guid ArtifactId,
    Guid ArtifactRevisionId,
    int StrategyId,
    string StrategyVersion) : ICommand<CreateChunkBatchResponse>;