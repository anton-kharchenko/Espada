using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Artifacts.Queries.GetArtifactRevisionById
{
    internal sealed class GetArtifactRevisionByIdQueryHandler(
        IArtifactRepository artifactRepository,
        IArtifactRevisionRepository artifactRevisionRepository)
        : IQueryHandler<
            GetArtifactRevisionByIdQuery,
            GetArtifactRevisionByIdResponse>
    {
        public async Task<DomainResult<GetArtifactRevisionByIdResponse>> Handle(
            GetArtifactRevisionByIdQuery request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult<GetArtifactRevisionByIdResponse>.Failure(
                    WorkspaceApplicationErrors.InvalidId);
            }

            if (request.ArtifactId == Guid.Empty)
            {
                return DomainResult<GetArtifactRevisionByIdResponse>.Failure(
                    ArtifactApplicationErrors.InvalidId);
            }

            if (request.ArtifactRevisionId == Guid.Empty)
            {
                return DomainResult<GetArtifactRevisionByIdResponse>.Failure(
                    ArtifactRevisionApplicationErrors.InvalidId);
            }

            ArtifactId artifactId =
                ArtifactId.Create(request.ArtifactId);

            Artifact? artifact =
                await artifactRepository.GetByIdAsync(
                    artifactId,
                    cancellationToken);

            if (artifact is null)
            {
                return DomainResult<GetArtifactRevisionByIdResponse>.Failure(
                    ArtifactApplicationErrors.NotFound(request.ArtifactId));
            }

            if (artifact.WorkspaceId.Value != request.WorkspaceId)
            {
                return DomainResult<GetArtifactRevisionByIdResponse>.Failure(
                    ArtifactApplicationErrors.NotFoundInWorkspace(
                        request.ArtifactId,
                        request.WorkspaceId));
            }

            ArtifactRevisionId revisionId =
                ArtifactRevisionId.Create(request.ArtifactRevisionId);

            ArtifactRevision? revision =
                await artifactRevisionRepository.GetByIdAsync(
                    revisionId,
                    cancellationToken);

            if (revision is null)
            {
                return DomainResult<GetArtifactRevisionByIdResponse>.Failure(
                    ArtifactRevisionApplicationErrors.NotFound(
                        request.ArtifactRevisionId));
            }

            if (!revision.ArtifactId.Equals(artifact.Id))
            {
                return DomainResult<GetArtifactRevisionByIdResponse>.Failure(
                    ArtifactRevisionApplicationErrors.NotFoundInArtifact(
                        request.ArtifactRevisionId,
                        request.ArtifactId));
            }

            GetArtifactRevisionByIdResponse response = new(
                revision.Id.Value,
                revision.ArtifactId.Value,
                revision.Number.Value,
                revision.Content.Value,
                revision.ContentHash.Value,
                revision.SizeInBytes,
                revision.CreatedAtUtc);

            return DomainResult<GetArtifactRevisionByIdResponse>.Success(
                response);
        }
    }
}