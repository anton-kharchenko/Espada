using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Jobs;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Imports.Commands.CancelImport
{
    internal sealed class CancelImportCommandHandler(
        IImportJobRepository importJobRepository,
        IUnitOfWork unitOfWork,
        IClockService clockService,
        IJobQueue jobQueue) : ICommandHandler<CancelImportCommand>
    {
        public async Task<DomainResult> Handle(
            CancelImportCommand request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure(WorkspaceApplicationErrors.InvalidId);
            }

            if (request.ImportJobId == Guid.Empty)
            {
                return DomainResult.Failure(ImportJobApplicationErrors.InvalidId);
            }

            ImportJobId importJobId = ImportJobId.Create(request.ImportJobId);

            ImportJob? importJob = await importJobRepository.GetByIdAsync(
                importJobId,
                cancellationToken);

            if (importJob is null)
            {
                return DomainResult.Failure(ImportJobApplicationErrors.NotFound(request.ImportJobId));
            }

            if (importJob.WorkspaceId.Value != request.WorkspaceId)
            {
                return DomainResult.Failure(
                    ImportJobApplicationErrors.NotFoundInWorkspace(
                        request.ImportJobId,
                        request.WorkspaceId));
            }

            DomainResult cancelResult = importJob.Cancel(clockService.UtcNow);

            if (cancelResult.IsFailure)
            {
                return cancelResult;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await jobQueue.CancelPendingAsync(importJob.Id, cancellationToken);

            return DomainResult.Success();
        }
    }
}