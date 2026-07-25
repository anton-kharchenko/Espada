using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Artifacts.Queries.ListArtifactRevisions
{
    internal sealed class ListArtifactRevisionsQueryHandler(
        IArtifactRepository artifactRepository,
        IArtifactRevisionRepository artifactRevisionRepository)
        : IQueryHandler<
            ListArtifactRevisionsQuery,
            ListArtifactRevisionsResponse>
    {
        public async Task<DomainResult<ListArtifactRevisionsResponse>> Handle(
            ListArtifactRevisionsQuery request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult<ListArtifactRevisionsResponse>.Failure(
                    WorkspaceApplicationErrors.InvalidId);
            }

            if (request.ArtifactId == Guid.Empty)
            {
                return DomainResult<ListArtifactRevisionsResponse>.Failure(
                    ArtifactApplicationErrors.InvalidId);
            }

            ArtifactId artifactId =
                ArtifactId.Create(request.ArtifactId);

            Artifact? artifact =
                await artifactRepository.GetByIdAsync(
                    artifactId,
                    cancellationToken);

            if (artifact is null)
            {
                return DomainResult<ListArtifactRevisionsResponse>.Failure(
                    ArtifactApplicationErrors.NotFound(request.ArtifactId));
            }

            if (artifact.WorkspaceId.Value != request.WorkspaceId)
            {
                return DomainResult<ListArtifactRevisionsResponse>.Failure(
                    ArtifactApplicationErrors.NotFoundInWorkspace(
                        request.ArtifactId,
                        request.WorkspaceId));
            }

            IReadOnlyList<ArtifactRevision> revisions =
                await artifactRevisionRepository.ListByArtifactIdAsync(
                    artifact.Id,
                    cancellationToken);

            ArtifactRevisionListItemResponse[] items =
                revisions
                    .OrderByDescending(revision => revision.Number.Value)
                    .Select(revision => new ArtifactRevisionListItemResponse(
                        revision.Id.Value,
                        revision.Number.Value,
                        revision.ContentHash.Value,
                        revision.SizeInBytes,
                        revision.CreatedAtUtc))
                    .ToArray();

            ListArtifactRevisionsResponse response =
                new(items);

            return DomainResult<ListArtifactRevisionsResponse>.Success(
                response);
        }
    }
}