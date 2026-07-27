using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Imports.Commands.RequestImport
{
    internal sealed class RequestImportCommandHandler(ISourceRepository sourceRepository, IImportJobRepository importJobRepository, IUnitOfWork unitOfWork, IClockService clockService)
        : ICommandHandler<RequestImportCommand, RequestImportResponse>
    {
        public async Task<DomainResult<RequestImportResponse>> Handle(RequestImportCommand request, CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure<RequestImportResponse>(WorkspaceApplicationErrors.InvalidId);
            }

            if (request.SourceId == Guid.Empty)
            {
                return DomainResult.Failure<RequestImportResponse>(SourceApplicationErrors.InvalidId);
            }

            SourceId sourceId = SourceId.Create(request.SourceId);

            Source? source = await sourceRepository.GetByIdAsync(sourceId, cancellationToken);

            if (source is null)
            {
                return DomainResult.Failure<RequestImportResponse>(SourceApplicationErrors.NotFound(request.SourceId));
            }

            if (source.WorkspaceId.Value != request.WorkspaceId)
            {
                return DomainResult.Failure<RequestImportResponse>(SourceApplicationErrors.NotFoundInWorkspace(request.SourceId, request.WorkspaceId));
            }

            ImportJobId importJobId = ImportJobId.Create(Guid.NewGuid());

            DomainResult<ImportJob> importJobResult = ImportJob.Request(importJobId, source.Id, source.WorkspaceId, clockService.UtcNow);

            if (importJobResult.IsFailure)
            {
                return DomainResult.Failure<RequestImportResponse>(importJobResult.Error);
            }

            ImportJob importJob = importJobResult.Value;

            await importJobRepository.AddAsync(importJob, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            RequestImportResponse response = new(importJob.Id.Value);

            return DomainResult.Success(response);
        }
    }
}