using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Chunks.Queries.GetChunkById;

internal sealed class GetChunkByIdQueryHandler(IChunkRepository chunkRepository) : IQueryHandler<GetChunkByIdQuery, GetChunkByIdResponse>
{
    public async Task<DomainResult<GetChunkByIdResponse>> Handle(GetChunkByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.WorkspaceId == Guid.Empty)
        {
            return DomainResult<GetChunkByIdResponse>.Failure(WorkspaceApplicationErrors.InvalidId);
        }

        if (request.ChunkId == Guid.Empty)
        {
            return DomainResult<GetChunkByIdResponse>.Failure(ChunkApplicationErrors.InvalidId);
        }

        Chunk? chunk = await chunkRepository.GetByIdAsync(ChunkId.Create(request.ChunkId), cancellationToken);

        if (chunk is null)
        {
            return DomainResult<GetChunkByIdResponse>.Failure(ChunkApplicationErrors.NotFound(request.ChunkId));
        }

        if (chunk.WorkspaceId.Value != request.WorkspaceId)
        {
            return DomainResult<GetChunkByIdResponse>.Failure(ChunkApplicationErrors.NotFoundInWorkspace(request.ChunkId, request.WorkspaceId));
        }

        GetChunkByIdResponse response = new(chunk.Id.Value, chunk.BatchId.Value, chunk.WorkspaceId.Value, chunk.ArtifactId.Value, chunk.ArtifactRevisionId.Value, chunk.Number.Value, chunk.Content.Value, chunk.ContentHash.Value, chunk.SizeInBytes, chunk.CharacterCount, chunk.SourceSpan?.Start, chunk.SourceSpan?.Length, chunk.Strategy.Id, chunk.Strategy.Name, chunk.StrategyVersion.Value, chunk.CreatedAtUtc);
        return DomainResult<GetChunkByIdResponse>.Success(response);
    }
}