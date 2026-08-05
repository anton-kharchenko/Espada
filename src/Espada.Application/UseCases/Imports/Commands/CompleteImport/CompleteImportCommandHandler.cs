using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Imports.Commands.CompleteImport
{
    internal sealed class CompleteImportCommandHandler(
        IImportJobRepository importJobRepository,
        IUnitOfWork unitOfWork,
        IClockService clockService)
        : ICommandHandler<CompleteImportCommand>
    {
        public async Task<DomainResult> Handle(CompleteImportCommand request, CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure(WorkspaceApplicationErrors.InvalidId);
            }

            if (request.ImportJobId == Guid.Empty)
            {
                return DomainResult.Failure(ImportJobApplicationErrors.InvalidId);
            }

            if (request.ArtifactId == Guid.Empty)
            {
                return DomainResult.Failure(ImportJobApplicationErrors.InvalidArtifactId);
            }

            if (request.ArtifactRevisionId == Guid.Empty)
            {
                return DomainResult.Failure(ImportJobApplicationErrors.InvalidArtifactRevisionId);
            }

            ImportJobId importJobId = ImportJobId.Create(request.ImportJobId);

            ImportJob? importJob = await importJobRepository.GetByIdAsync(importJobId, cancellationToken);

            if (importJob is null)
            {
                return DomainResult.Failure(ImportJobApplicationErrors.NotFound(request.ImportJobId));
            }

            if (importJob.WorkspaceId.Value != request.WorkspaceId)
            {
                return DomainResult.Failure(
                    ImportJobApplicationErrors.NotFoundInWorkspace(request.ImportJobId, request.WorkspaceId));
            }

            ArtifactId artifactId = ArtifactId.Create(request.ArtifactId);

            ArtifactRevisionId artifactRevisionId = ArtifactRevisionId.Create(request.ArtifactRevisionId);

            DomainResult completeResult = importJob.Complete(artifactId, artifactRevisionId, clockService.UtcNow);

            if (completeResult.IsFailure)
            {
                return completeResult;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return DomainResult.Success();
        }
    }
}