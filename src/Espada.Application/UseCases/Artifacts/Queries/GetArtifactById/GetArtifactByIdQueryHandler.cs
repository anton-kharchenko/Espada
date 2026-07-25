using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Artifacts.Queries.GetArtifactById
{
    internal sealed class GetArtifactByIdQueryHandler(
        IArtifactRepository artifactRepository)
        : IQueryHandler<GetArtifactByIdQuery, GetArtifactByIdResponse>
    {
        public async Task<DomainResult<GetArtifactByIdResponse>> Handle(
            GetArtifactByIdQuery request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult<GetArtifactByIdResponse>.Failure(
                    WorkspaceApplicationErrors.InvalidId);
            }

            if (request.ArtifactId == Guid.Empty)
            {
                return DomainResult<GetArtifactByIdResponse>.Failure(
                    ArtifactApplicationErrors.InvalidId);
            }

            ArtifactId artifactId = ArtifactId.Create(request.ArtifactId);

            Artifact? artifact = await artifactRepository.GetByIdAsync(
                artifactId,
                cancellationToken);

            if (artifact is null)
            {
                return DomainResult<GetArtifactByIdResponse>.Failure(
                    ArtifactApplicationErrors.NotFound(request.ArtifactId));
            }

            if (artifact.WorkspaceId.Value != request.WorkspaceId)
            {
                return DomainResult<GetArtifactByIdResponse>.Failure(
                    ArtifactApplicationErrors.NotFoundInWorkspace(
                        request.ArtifactId,
                        request.WorkspaceId));
            }

            GetArtifactByIdResponse response = new(
                artifact.Id.Value,
                artifact.WorkspaceId.Value,
                artifact.Title.Value,
                artifact.Type.Id,
                artifact.Type.Name,
                artifact.Status.Id,
                artifact.Status.Name,
                artifact.CurrentRevisionId?.Value,
                artifact.CurrentRevisionNumber?.Value,
                artifact.RevisionCount,
                artifact.CreatedAtUtc,
                artifact.UpdatedAtUtc,
                artifact.ArchivedAtUtc);

            return DomainResult<GetArtifactByIdResponse>.Success(response);
        }
    }
}