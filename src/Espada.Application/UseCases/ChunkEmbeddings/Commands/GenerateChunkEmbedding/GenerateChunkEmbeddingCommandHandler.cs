using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Embedding;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.Models;
using Espada.Application.UseCases.ChunkEmbeddings.Commands.CreateChunkEmbedding;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.ChunkEmbeddings.Commands.GenerateChunkEmbedding;

internal sealed class GenerateChunkEmbeddingCommandHandler(
    IChunkRepository chunkRepository,
    IChunkEmbeddingRepository chunkEmbeddingRepository,
    IEmbeddingVectorStore embeddingVectorStore,
    IEmbeddingGeneratorService embeddingGeneratorService,
    IUnitOfWork unitOfWork,
    IClockService clockService,
    IMapper mapper) : ICommandHandler<GenerateChunkEmbeddingCommand, CreateChunkEmbeddingResponse>
{
    public async Task<DomainResult<CreateChunkEmbeddingResponse>> Handle(GenerateChunkEmbeddingCommand request, CancellationToken cancellationToken)
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

        Chunk? chunk = await chunkRepository.GetByIdAsync(ChunkId.Create(request.ChunkId), cancellationToken);
        if (chunk is null)
        {
            return DomainResult<CreateChunkEmbeddingResponse>.Failure(ChunkApplicationErrors.NotFound(request.ChunkId));
        }

        if (chunk.WorkspaceId.Value != request.WorkspaceId)
        {
            return DomainResult<CreateChunkEmbeddingResponse>.Failure(ChunkApplicationErrors.NotFoundInWorkspace(request.ChunkId, request.WorkspaceId));
        }

        GeneratedEmbedding generated = await embeddingGeneratorService.GenerateAsync(modelResult.Value.Identifier, modelResult.Value.Version, chunk.Content.Value, cancellationToken);
        if (generated.Vector.Count == 0 || generated.Vector.Any(value => !float.IsFinite(value)))
        {
            return DomainResult<CreateChunkEmbeddingResponse>.Failure(ChunkEmbeddingApplicationErrors.VectorContainsNonFiniteValue);
        }

        IReadOnlyList<int> storedDimensions = await chunkEmbeddingRepository.ListDimensionsAsync(chunk.WorkspaceId, modelResult.Value, cancellationToken);
        int dimensions = generated.Vector.Count;

        if (storedDimensions.Count > 1)
        {
            return DomainResult<CreateChunkEmbeddingResponse>.Failure(ChunkEmbeddingApplicationErrors.InconsistentModelDimensions(modelResult.Value.Identifier, modelResult.Value.Version));
        }

        if (storedDimensions.Count == 1 && storedDimensions[0] != dimensions)
        {
            return DomainResult<CreateChunkEmbeddingResponse>.Failure(ChunkEmbeddingApplicationErrors.DimensionMismatch(modelResult.Value.Identifier, modelResult.Value.Version, storedDimensions[0], dimensions));
        }

        ChunkEmbedding? embedding = await chunkEmbeddingRepository.GetByChunkIdAsync(chunk.Id, modelResult.Value, cancellationToken);
        if (embedding is null)
        {
            DomainResult<EmbeddingDimensions> dimensionsResult = EmbeddingDimensions.Create(dimensions);
            DomainResult<ChunkEmbedding> embeddingResult = ChunkEmbedding.Create(ChunkEmbeddingId.New(), chunk.WorkspaceId, chunk.Id, chunk.ContentHash, modelResult.Value, dimensionsResult.Value, clockService.UtcNow);

            embedding = embeddingResult.Value;
            await chunkEmbeddingRepository.AddAsync(embedding, cancellationToken);
        }

        await embeddingVectorStore.UpsertAsync(embedding.Id, generated.Vector, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return DomainResult<CreateChunkEmbeddingResponse>.Success(mapper.Map<CreateChunkEmbeddingResponse>(embedding));
    }
}