using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.Contracts.Billing;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Imports.Commands.RequestImport;

internal sealed class RequestImportCommandHandler(ISourceRepository sourceRepository, IImportJobRepository importJobRepository, IUnitOfWork unitOfWork, IClockService clockService, IImportAdmissionPolicy importAdmissionPolicy) : ICommandHandler<RequestImportCommand, RequestImportResponse>
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

        if (string.IsNullOrWhiteSpace(request.Options.EmbeddingModel))
        {
            return DomainResult.Failure<RequestImportResponse>(ImportJobApplicationErrors.EmbeddingModelRequired);
        }

        string? denialReason = await importAdmissionPolicy.GetDenialReasonAsync(request.WorkspaceId, cancellationToken);
        if (denialReason is not null)
        {
            return DomainResult.Failure<RequestImportResponse>(
                ImportJobApplicationErrors.CloudImportBlocked(denialReason));
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

        string requestFingerprint = RequestImportFingerprint.Create(request.SourceId, request.Options);
        ImportJob? existing = await importJobRepository.GetByIdempotencyKeyAsync(source.WorkspaceId, request.IdempotencyKey, cancellationToken);

        if (existing is not null)
        {
            return existing.RequestFingerprint == requestFingerprint
                ? DomainResult.Success(new RequestImportResponse(existing.Id.Value))
                : DomainResult.Failure<RequestImportResponse>(ImportJobApplicationErrors.IdempotencyConflict);
        }

        ImportJobId importJobId = ImportJobId.Create(Guid.NewGuid());

        DomainResult<ImportJob> importJobResult = ImportJob.Request(importJobId, source.Id, source.WorkspaceId, clockService.UtcNow, request.IdempotencyKey, requestFingerprint, RequestImportFingerprint.SerializeOptions(request.Options));

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