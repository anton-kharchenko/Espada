using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Chunks.Commands.CreateChunks;

internal sealed class CreateChunksCommandHandler(
    IChunkBatchRepository chunkBatchRepository,
    IChunkRepository chunkRepository,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CreateChunksCommand, CreateChunksResponse>
{
    public async Task<DomainResult<CreateChunksResponse>> Handle(CreateChunksCommand request, CancellationToken cancellationToken)
    {
        if (request.WorkspaceId == Guid.Empty)
        {
            return DomainResult<CreateChunksResponse>.Failure(WorkspaceApplicationErrors.InvalidId);
        }

        if (request.ChunkBatchId == Guid.Empty)
        {
            return DomainResult<CreateChunksResponse>.Failure(ChunkBatchApplicationErrors.InvalidId);
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            return DomainResult<CreateChunksResponse>.Failure(ChunkApplicationErrors.ItemsEmpty);
        }

        int[] numbers = request.Items.Select(item => item.Number).OrderBy(number => number).ToArray();

        if (!numbers.SequenceEqual(Enumerable.Range(1, request.Items.Count)))
        {
            return DomainResult<CreateChunksResponse>.Failure(ChunkApplicationErrors.NumbersNotSequential);
        }

        ChunkBatchId batchId = ChunkBatchId.Create(request.ChunkBatchId);
        ChunkBatch? batch = await chunkBatchRepository.GetByIdAsync(batchId, cancellationToken);

        if (batch is null)
        {
            return DomainResult<CreateChunksResponse>.Failure(ChunkBatchApplicationErrors.NotFound(request.ChunkBatchId));
        }

        if (batch.WorkspaceId.Value != request.WorkspaceId)
        {
            return DomainResult<CreateChunksResponse>.Failure(ChunkBatchApplicationErrors.NotFoundInWorkspace(request.ChunkBatchId, request.WorkspaceId));
        }

        DateTimeOffset startedAtUtc = clock.UtcNow;
        DomainResult startResult = batch.Start(startedAtUtc);

        if (startResult.IsFailure)
        {
            return DomainResult<CreateChunksResponse>.Failure(startResult.Error);
        }

        List<Chunk> chunks = new(request.Items.Count);

        foreach (CreateChunkItem item in request.Items.OrderBy(value => value.Number))
        {
            if (item.SourceStart.HasValue != item.SourceLength.HasValue)
            {
                return DomainResult<CreateChunksResponse>.Failure(ChunkApplicationErrors.SourceSpanIncomplete);
            }

            DomainResult<ChunkNumber> numberResult = ChunkNumber.Create(item.Number);

            if (numberResult.IsFailure)
            {
                return DomainResult<CreateChunksResponse>.Failure(numberResult.Error);
            }

            DomainResult<ChunkContent> contentResult = ChunkContent.Create(item.Content);

            if (contentResult.IsFailure)
            {
                return DomainResult<CreateChunksResponse>.Failure(contentResult.Error);
            }

            SourceTextSpan? sourceSpan = null;

            if (item.SourceStart.HasValue && item.SourceLength.HasValue)
            {
                DomainResult<SourceTextSpan> sourceSpanResult = SourceTextSpan.Create(item.SourceStart.Value, item.SourceLength.Value);

                if (sourceSpanResult.IsFailure)
                {
                    return DomainResult<CreateChunksResponse>.Failure(sourceSpanResult.Error);
                }

                sourceSpan = sourceSpanResult.Value;
            }

            DomainResult<Chunk> chunkResult = Chunk.Create(ChunkId.New(), batch.Id, batch.WorkspaceId, batch.ArtifactId, batch.ArtifactRevisionId, numberResult.Value, contentResult.Value, sourceSpan, batch.Strategy, batch.StrategyVersion, startedAtUtc);

            if (chunkResult.IsFailure)
            {
                return DomainResult<CreateChunksResponse>.Failure(chunkResult.Error);
            }

            chunks.Add(chunkResult.Value);
        }

        DateTimeOffset completedAtUtc = clock.UtcNow;
        DomainResult completeResult = batch.Complete(chunks.Count, completedAtUtc);

        if (completeResult.IsFailure)
        {
            return DomainResult<CreateChunksResponse>.Failure(completeResult.Error);
        }

        await chunkRepository.AddRangeAsync(chunks, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        CreatedChunkResponse[] items = chunks.Select(chunk => new CreatedChunkResponse(chunk.Id.Value, chunk.Number.Value, chunk.ContentHash.Value, chunk.SizeInBytes, chunk.CharacterCount)).ToArray();
        return DomainResult<CreateChunksResponse>.Success(new CreateChunksResponse(batch.Id.Value, chunks.Count, completedAtUtc, items));
    }
}