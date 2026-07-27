using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Artifacts.Commands.RenameArtifact
{
    internal sealed class RenameArtifactCommandHandler(
        IArtifactRepository artifactRepository,
        IUnitOfWork unitOfWork,
        IClockService clockService) : ICommandHandler<RenameArtifactCommand>
    {
        public async Task<DomainResult> Handle(
            RenameArtifactCommand request,
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

            DomainResult<ArtifactTitle> titleResult =
                ArtifactTitle.Create(request.Title);

            if (titleResult.IsFailure)
            {
                return DomainResult.Failure(titleResult.Error);
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

            DomainResult renameResult = artifact.Rename(
                titleResult.Value,
                clockService.UtcNow);

            if (renameResult.IsFailure)
            {
                return renameResult;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return DomainResult.Success();
        }
    }
}