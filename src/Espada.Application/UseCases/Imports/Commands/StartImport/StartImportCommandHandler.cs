using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Imports.Commands.StartImport;

internal sealed class StartImportCommandHandler(IImportJobRepository importJobRepository, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<StartImportCommand>
{
    public async Task<DomainResult> Handle(StartImportCommand request, CancellationToken cancellationToken)
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

        ImportJob? importJob = await importJobRepository.GetByIdAsync(importJobId, cancellationToken);

        if (importJob is null)
        {
            return DomainResult.Failure(ImportJobApplicationErrors.NotFound(request.ImportJobId));
        }

        if (importJob.WorkspaceId.Value != request.WorkspaceId)
        {
            return DomainResult.Failure(ImportJobApplicationErrors.NotFoundInWorkspace(request.ImportJobId, request.WorkspaceId));
        }

        DomainResult startResult = importJob.Start(clock.UtcNow);

        if (startResult.IsFailure)
        {
            return startResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return DomainResult.Success();
    }
}