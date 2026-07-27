using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.ChunkEmbeddings.Queries.GetChunkEmbeddingByChunkId;

internal sealed class GetChunkEmbeddingByChunkIdQueryHandler(
    IChunkRepository chunkRepository,
    IChunkEmbeddingRepository chunkEmbeddingRepository,
    IEmbeddingVectorStore embeddingVectorStore) : IQueryHandler<GetChunkEmbeddingByChunkIdQuery, GetChunkEmbeddingByChunkIdResponse>
{
    public async Task<DomainResult<GetChunkEmbeddingByChunkIdResponse>> Handle(GetChunkEmbeddingByChunkIdQuery request, CancellationToken cancellationToken)
    {
        if (request.WorkspaceId == Guid.Empty)
        {
            return DomainResult<GetChunkEmbeddingByChunkIdResponse>.Failure(WorkspaceApplicationErrors.InvalidId);
        }

        if (request.ChunkId == Guid.Empty)
        {
            return DomainResult<GetChunkEmbeddingByChunkIdResponse>.Failure(ChunkApplicationErrors.InvalidId);
        }

        ChunkId chunkId = ChunkId.Create(request.ChunkId);
        Chunk? chunk = await chunkRepository.GetByIdAsync(chunkId, cancellationToken);

        if (chunk is null)
        {
            return DomainResult<GetChunkEmbeddingByChunkIdResponse>.Failure(ChunkApplicationErrors.NotFound(request.ChunkId));
        }

        if (chunk.WorkspaceId.Value != request.WorkspaceId)
        {
            return DomainResult<GetChunkEmbeddingByChunkIdResponse>.Failure(ChunkApplicationErrors.NotFoundInWorkspace(request.ChunkId, request.WorkspaceId));
        }

        DomainResult<EmbeddingModel> modelResult = EmbeddingModel.Create(request.ModelIdentifier, request.ModelVersion);

        if (modelResult.IsFailure)
        {
            return DomainResult<GetChunkEmbeddingByChunkIdResponse>.Failure(modelResult.Error);
        }

        ChunkEmbedding? embedding = await chunkEmbeddingRepository.GetByChunkIdAsync(chunk.Id, modelResult.Value, cancellationToken);

        if (embedding is null)
        {
            return DomainResult<GetChunkEmbeddingByChunkIdResponse>.Failure(ChunkEmbeddingApplicationErrors.NotFoundForChunk(request.ChunkId));
        }

        IReadOnlyList<float>? vector = await embeddingVectorStore.GetByIdAsync(embedding.Id, cancellationToken);

        if (vector is null)
        {
            return DomainResult<GetChunkEmbeddingByChunkIdResponse>.Failure(ChunkEmbeddingApplicationErrors.VectorNotFound(embedding.Id.Value));
        }

        GetChunkEmbeddingByChunkIdResponse response = new(embedding.Id.Value, embedding.ChunkId.Value, embedding.ChunkContentHash.Value, embedding.Model.Identifier, embedding.Model.Version, embedding.Dimensions.Value, vector, embedding.CreatedAtUtc);
        return DomainResult<GetChunkEmbeddingByChunkIdResponse>.Success(response);
    }
}