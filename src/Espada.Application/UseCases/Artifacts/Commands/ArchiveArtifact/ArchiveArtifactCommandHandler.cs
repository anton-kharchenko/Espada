using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Artifacts.Commands.ArchiveArtifact
{
    internal sealed class ArchiveArtifactCommandHandler(
        IArtifactRepository artifactRepository,
        IUnitOfWork unitOfWork,
        IClock clock) : ICommandHandler<ArchiveArtifactCommand>
    {
        public async Task<DomainResult> Handle(
            ArchiveArtifactCommand request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure(
                    WorkspaceApplicationErrors.InvalidId);
            }

            if (request.ArtifactId == Guid.Empty)
            {
                return DomainResult.Failure(
                    ArtifactApplicationErrors.InvalidId);
            }

            ArtifactId artifactId = ArtifactId.Create(request.ArtifactId);

            Artifact? artifact = await artifactRepository.GetByIdAsync(
                artifactId,
                cancellationToken);

            if (artifact is null)
            {
                return DomainResult.Failure(
                    ArtifactApplicationErrors.NotFound(request.ArtifactId));
            }

            if (artifact.WorkspaceId.Value != request.WorkspaceId)
            {
                return DomainResult.Failure(
                    ArtifactApplicationErrors.NotFoundInWorkspace(
                        request.ArtifactId,
                        request.WorkspaceId));
            }

            DomainResult archiveResult = artifact.Archive(clock.UtcNow);

            if (archiveResult.IsFailure)
            {
                return archiveResult;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return DomainResult.Success();
        }
    }
}