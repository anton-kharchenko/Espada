using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Artifacts.Commands.CreateArtifact
{
    internal sealed class CreateArtifactCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IArtifactRepository artifactRepository,
        IArtifactRevisionRepository artifactRevisionRepository,
        IUnitOfWork unitOfWork,
        IClock clock) : ICommandHandler<CreateArtifactCommand, CreateArtifactResponse>
    {
        public async Task<DomainResult<CreateArtifactResponse>> Handle(CreateArtifactCommand request, CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult<CreateArtifactResponse>.Failure(WorkspaceApplicationErrors.InvalidId);
            }

            DomainResult<ArtifactTitle> titleResult = ArtifactTitle.Create(request.Title);

            if (titleResult.IsFailure)
            {
                return DomainResult<CreateArtifactResponse>.Failure(titleResult.Error);
            }

            DomainResult<ArtifactContent> contentResult = ArtifactContent.Create(request.Content);

            if (contentResult.IsFailure)
            {
                return DomainResult<CreateArtifactResponse>.Failure(contentResult.Error);
            }

            ArtifactType? artifactType = Enumeration
                .GetAll<ArtifactType>()
                .SingleOrDefault(type => type.Id == request.TypeId);

            if (artifactType is null)
            {
                return DomainResult<CreateArtifactResponse>.Failure(ArtifactApplicationErrors.UnsupportedType(request.TypeId));
            }

            WorkspaceId workspaceId = WorkspaceId.Create(request.WorkspaceId);

            Workspace? workspace = await workspaceRepository.GetByIdAsync(workspaceId, cancellationToken);

            if (workspace is null)
            {
                return DomainResult<CreateArtifactResponse>.Failure(WorkspaceApplicationErrors.NotFound(request.WorkspaceId));
            }

            ArtifactId artifactId = ArtifactId.Create(Guid.NewGuid());

            DateTimeOffset createdAtUtc = clock.UtcNow;

            DomainResult<Artifact> artifactResult = Artifact.Create(artifactId, workspace.Id, titleResult.Value, artifactType, createdAtUtc);

            if (artifactResult.IsFailure)
            {
                return DomainResult<CreateArtifactResponse>.Failure(artifactResult.Error);
            }

            Artifact artifact = artifactResult.Value;

            ArtifactRevisionId artifactRevisionId = ArtifactRevisionId.Create(Guid.NewGuid());

            DomainResult<ArtifactRevision> revisionResult = artifact.CreateRevision(artifactRevisionId, contentResult.Value, createdAtUtc);

            if (revisionResult.IsFailure)
            {
                return DomainResult<CreateArtifactResponse>.Failure(revisionResult.Error);
            }

            ArtifactRevision revision = revisionResult.Value;

            await artifactRepository.AddAsync(artifact, cancellationToken);

            await artifactRevisionRepository.AddAsync(revision, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            CreateArtifactResponse response = new(
                artifact.Id.Value,
                revision.Id.Value,
                revision.Number.Value,
                revision.ContentHash.Value,
                revision.SizeInBytes,
                createdAtUtc);

            return DomainResult<CreateArtifactResponse>.Success(response);
        }
    }
}