using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Artifacts.Commands.AddArtifactRevision
{
    internal sealed class AddArtifactRevisionCommandHandler(
        IArtifactRepository artifactRepository,
        IArtifactRevisionRepository artifactRevisionRepository,
        IUnitOfWork unitOfWork,
        IClock clock)
        : ICommandHandler<AddArtifactRevisionCommand, AddArtifactRevisionResponse>
    {
        public async Task<DomainResult<AddArtifactRevisionResponse>> Handle(
            AddArtifactRevisionCommand request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult<AddArtifactRevisionResponse>.Failure(WorkspaceApplicationErrors.InvalidId);
            }

            if (request.ArtifactId == Guid.Empty)
            {
                return DomainResult<AddArtifactRevisionResponse>.Failure(ArtifactApplicationErrors.InvalidId);
            }

            DomainResult<ArtifactContent> contentResult = ArtifactContent.Create(request.Content);

            if (contentResult.IsFailure)
            {
                return DomainResult<AddArtifactRevisionResponse>.Failure(contentResult.Error);
            }

            ArtifactId artifactId = ArtifactId.Create(request.ArtifactId);

            Artifact? artifact = await artifactRepository.GetByIdAsync(artifactId, cancellationToken);

            if (artifact is null)
            {
                return DomainResult<AddArtifactRevisionResponse>.Failure(ArtifactApplicationErrors.NotFound(request.ArtifactId));
            }

            if (artifact.WorkspaceId.Value != request.WorkspaceId)
            {
                return DomainResult<AddArtifactRevisionResponse>.Failure(ArtifactApplicationErrors.NotFoundInWorkspace(request.ArtifactId, request.WorkspaceId));
            }

            ArtifactRevisionId revisionId = ArtifactRevisionId.Create(Guid.NewGuid());

            DateTimeOffset createdAtUtc = clock.UtcNow;

            DomainResult<ArtifactRevision> revisionResult = artifact.CreateRevision(revisionId, contentResult.Value, createdAtUtc);

            if (revisionResult.IsFailure)
            {
                return DomainResult<AddArtifactRevisionResponse>.Failure(revisionResult.Error);
            }

            ArtifactRevision revision = revisionResult.Value;

            await artifactRevisionRepository.AddAsync(revision, cancellationToken);

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            AddArtifactRevisionResponse response = new(
                artifact.Id.Value,
                revision.Id.Value,
                revision.Number.Value,
                revision.ContentHash.Value,
                revision.SizeInBytes,
                revision.CreatedAtUtc);

            return DomainResult<AddArtifactRevisionResponse>.Success(
                response);
        }
    }
}