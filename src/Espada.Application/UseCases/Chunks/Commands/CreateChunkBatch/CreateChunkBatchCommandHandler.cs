using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Chunks.Commands.CreateChunkBatch;

internal sealed class CreateChunkBatchCommandHandler(
    IArtifactRepository artifactRepository,
    IArtifactRevisionRepository artifactRevisionRepository,
    IChunkBatchRepository chunkBatchRepository,
    IUnitOfWork unitOfWork,
    IClockService clockService) : ICommandHandler<CreateChunkBatchCommand, CreateChunkBatchResponse>
{
    public async Task<DomainResult<CreateChunkBatchResponse>> Handle(CreateChunkBatchCommand request, CancellationToken cancellationToken)
    {
        if (request.WorkspaceId == Guid.Empty)
        {
            return DomainResult<CreateChunkBatchResponse>.Failure(WorkspaceApplicationErrors.InvalidId);
        }

        if (request.ArtifactId == Guid.Empty)
        {
            return DomainResult<CreateChunkBatchResponse>.Failure(ArtifactApplicationErrors.InvalidId);
        }

        if (request.ArtifactRevisionId == Guid.Empty)
        {
            return DomainResult<CreateChunkBatchResponse>.Failure(ArtifactRevisionApplicationErrors.InvalidId);
        }

        ChunkingStrategyType? strategy = Enumeration.GetAll<ChunkingStrategyType>().SingleOrDefault(value => value.Id == request.StrategyId);

        if (strategy is null)
        {
            return DomainResult<CreateChunkBatchResponse>.Failure(ChunkBatchApplicationErrors.UnsupportedStrategy(request.StrategyId));
        }

        DomainResult<ChunkingVersion> versionResult = ChunkingVersion.Create(request.StrategyVersion);

        if (versionResult.IsFailure)
        {
            return DomainResult<CreateChunkBatchResponse>.Failure(versionResult.Error);
        }

        ArtifactId artifactId = ArtifactId.Create(request.ArtifactId);
        Artifact? artifact = await artifactRepository.GetByIdAsync(artifactId, cancellationToken);

        if (artifact is null)
        {
            return DomainResult<CreateChunkBatchResponse>.Failure(ArtifactApplicationErrors.NotFound(request.ArtifactId));
        }

        if (artifact.WorkspaceId.Value != request.WorkspaceId)
        {
            return DomainResult<CreateChunkBatchResponse>.Failure(ArtifactApplicationErrors.NotFoundInWorkspace(request.ArtifactId, request.WorkspaceId));
        }

        ArtifactRevisionId revisionId = ArtifactRevisionId.Create(request.ArtifactRevisionId);
        ArtifactRevision? revision = await artifactRevisionRepository.GetByIdAsync(revisionId, cancellationToken);

        if (revision is null)
        {
            return DomainResult<CreateChunkBatchResponse>.Failure(ArtifactRevisionApplicationErrors.NotFound(request.ArtifactRevisionId));
        }

        if (!revision.ArtifactId.Equals(artifact.Id))
        {
            return DomainResult<CreateChunkBatchResponse>.Failure(ArtifactRevisionApplicationErrors.NotFoundInArtifact(request.ArtifactRevisionId, request.ArtifactId));
        }

        DateTimeOffset requestedAtUtc = clockService.UtcNow;
        DomainResult<ChunkBatch> batchResult = ChunkBatch.Request(ChunkBatchId.New(), artifact.WorkspaceId, artifact.Id, revision.Id, strategy, versionResult.Value, requestedAtUtc);

        if (batchResult.IsFailure)
        {
            return DomainResult<CreateChunkBatchResponse>.Failure(batchResult.Error);
        }

        ChunkBatch batch = batchResult.Value;
        await chunkBatchRepository.AddAsync(batch, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        CreateChunkBatchResponse response = new(batch.Id.Value, batch.ArtifactRevisionId.Value, batch.Strategy.Id, batch.Strategy.Name, batch.StrategyVersion.Value, batch.Status.Id, batch.Status.Name, batch.RequestedAtUtc);
        return DomainResult<CreateChunkBatchResponse>.Success(response);
    }
}