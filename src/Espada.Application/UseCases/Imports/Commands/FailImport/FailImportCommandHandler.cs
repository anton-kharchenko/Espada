using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Imports.Commands.FailImport
{
    internal sealed class FailImportCommandHandler(
        IImportJobRepository importJobRepository,
        IUnitOfWork unitOfWork,
        IClockService clockService) : ICommandHandler<FailImportCommand>
    {
        public async Task<DomainResult> Handle(FailImportCommand request, CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure(WorkspaceApplicationErrors.InvalidId);
            }

            if (request.ImportJobId == Guid.Empty)
            {
                return DomainResult.Failure(ImportJobApplicationErrors.InvalidId);
            }

            DomainResult<ImportFailure>
                failureResult = ImportFailure.Create(request.FailureCode, request.FailureReason);

            if (failureResult.IsFailure)
            {
                return DomainResult.Failure(failureResult.Error);
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

            DomainResult failResult = importJob.Fail(failureResult.Value, clockService.UtcNow);

            if (failResult.IsFailure)
            {
                return failResult;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return DomainResult.Success();
        }
    }
}