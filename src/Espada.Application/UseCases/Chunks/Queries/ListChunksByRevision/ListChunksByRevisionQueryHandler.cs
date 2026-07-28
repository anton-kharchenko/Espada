using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Chunks.Queries.ListChunksByRevision
{
    internal sealed class ListChunksByRevisionQueryHandler(
        IArtifactRepository artifactRepository,
        IArtifactRevisionRepository artifactRevisionRepository,
        IChunkRepository chunkRepository) : IQueryHandler<ListChunksByRevisionQuery, ListChunksByRevisionResponse>
    {
        public async Task<DomainResult<ListChunksByRevisionResponse>> Handle(ListChunksByRevisionQuery request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult<ListChunksByRevisionResponse>.Failure(WorkspaceApplicationErrors.InvalidId);
            }

            if (request.ArtifactRevisionId == Guid.Empty)
            {
                return DomainResult<ListChunksByRevisionResponse>.Failure(ArtifactRevisionApplicationErrors.InvalidId);
            }

            ArtifactRevisionId revisionId = ArtifactRevisionId.Create(request.ArtifactRevisionId);
            ArtifactRevision? revision = await artifactRevisionRepository.GetByIdAsync(revisionId, cancellationToken);

            if (revision is null)
            {
                return DomainResult<ListChunksByRevisionResponse>.Failure(
                    ArtifactRevisionApplicationErrors.NotFound(request.ArtifactRevisionId));
            }

            Artifact? artifact = await artifactRepository.GetByIdAsync(revision.ArtifactId, cancellationToken);

            if (artifact is null || artifact.WorkspaceId.Value != request.WorkspaceId)
            {
                return DomainResult<ListChunksByRevisionResponse>.Failure(
                    ArtifactRevisionApplicationErrors.NotFound(request.ArtifactRevisionId));
            }

            IReadOnlyList<Chunk> chunks =
                await chunkRepository.ListByArtifactRevisionIdAsync(revision.Id, cancellationToken);

            ChunkListItemResponse[] items = chunks
                .OrderBy(chunk => chunk.Number.Value)
                .Select(chunk => new ChunkListItemResponse(chunk.Id.Value, chunk.BatchId.Value, chunk.Number.Value,
                    chunk.ContentHash.Value, chunk.SizeInBytes, chunk.CharacterCount, chunk.SourceSpan?.Start,
                    chunk.SourceSpan?.Length, chunk.CreatedAtUtc))
                .ToArray();

            return DomainResult<ListChunksByRevisionResponse>.Success(new ListChunksByRevisionResponse(items));
        }
    }
}