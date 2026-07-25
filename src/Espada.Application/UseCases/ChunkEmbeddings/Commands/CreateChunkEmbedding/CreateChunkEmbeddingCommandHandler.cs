using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.ChunkEmbeddings.Commands.CreateChunkEmbedding;

internal sealed class CreateChunkEmbeddingCommandHandler(
    IChunkRepository chunkRepository,
    IChunkEmbeddingRepository chunkEmbeddingRepository,
    IEmbeddingVectorStore embeddingVectorStore,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CreateChunkEmbeddingCommand, CreateChunkEmbeddingResponse>
{
    public async Task<DomainResult<CreateChunkEmbeddingResponse>> Handle(CreateChunkEmbeddingCommand request, CancellationToken cancellationToken)
    {
        if (request.WorkspaceId == Guid.Empty)
        {
            return DomainResult<CreateChunkEmbeddingResponse>.Failure(WorkspaceApplicationErrors.InvalidId);
        }

        if (request.ChunkId == Guid.Empty)
        {
            return DomainResult<CreateChunkEmbeddingResponse>.Failure(ChunkApplicationErrors.InvalidId);
        }

        DomainResult<EmbeddingModel> modelResult = EmbeddingModel.Create(request.ModelIdentifier, request.ModelVersion);

        if (modelResult.IsFailure)
        {
            return DomainResult<CreateChunkEmbeddingResponse>.Failure(modelResult.Error);
        }

        IReadOnlyList<float> vector = request.Vector ?? [];
        DomainResult<EmbeddingDimensions> dimensionsResult = EmbeddingDimensions.Create(vector.Count);

        if (dimensionsResult.IsFailure)
        {
            return DomainResult<CreateChunkEmbeddingResponse>.Failure(dimensionsResult.Error);
        }

        Chunk? chunk = await chunkRepository.GetByIdAsync(ChunkId.Create(request.ChunkId), cancellationToken);

        if (chunk is null)
        {
            return DomainResult<CreateChunkEmbeddingResponse>.Failure(ChunkApplicationErrors.NotFound(request.ChunkId));
        }

        if (chunk.WorkspaceId.Value != request.WorkspaceId)
        {
            return DomainResult<CreateChunkEmbeddingResponse>.Failure(ChunkApplicationErrors.NotFoundInWorkspace(request.ChunkId, request.WorkspaceId));
        }

        DateTimeOffset createdAtUtc = clock.UtcNow;
        DomainResult<ChunkEmbedding> embeddingResult = ChunkEmbedding.Create(ChunkEmbeddingId.New(), chunk.WorkspaceId, chunk.Id, chunk.ContentHash, modelResult.Value, dimensionsResult.Value, createdAtUtc);

        if (embeddingResult.IsFailure)
        {
            return DomainResult<CreateChunkEmbeddingResponse>.Failure(embeddingResult.Error);
        }

        ChunkEmbedding embedding = embeddingResult.Value;
        await chunkEmbeddingRepository.AddAsync(embedding, cancellationToken);
        await embeddingVectorStore.AddAsync(embedding.Id, vector, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        CreateChunkEmbeddingResponse response = new(embedding.Id.Value, embedding.ChunkId.Value, embedding.ChunkContentHash.Value, embedding.Model.Identifier, embedding.Model.Version, embedding.Dimensions.Value, embedding.CreatedAtUtc);
        return DomainResult<CreateChunkEmbeddingResponse>.Success(response);
    }
}