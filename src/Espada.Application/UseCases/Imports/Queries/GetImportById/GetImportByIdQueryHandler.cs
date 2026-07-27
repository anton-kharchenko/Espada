using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Jobs;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Imports.Queries.GetImportById;

internal sealed class GetImportByIdQueryHandler(
    IImportJobRepository importJobRepository,
    IJobQueue jobQueue,
    IMapper mapper) : IQueryHandler<GetImportByIdQuery, GetImportByIdResponse>
{
    public async Task<DomainResult<GetImportByIdResponse>> Handle(
        GetImportByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.WorkspaceId == Guid.Empty)
        {
            return DomainResult.Failure<GetImportByIdResponse>(WorkspaceApplicationErrors.InvalidId);
        }

        if (request.ImportJobId == Guid.Empty)
        {
            return DomainResult.Failure<GetImportByIdResponse>(ImportJobApplicationErrors.InvalidId);
        }

        ImportJobId importJobId = ImportJobId.Create(request.ImportJobId);

        ImportJob? importJob = await importJobRepository.GetByIdAsync(importJobId, cancellationToken);

        if (importJob is null)
        {
            return DomainResult.Failure<GetImportByIdResponse>(
                ImportJobApplicationErrors.NotFound(request.ImportJobId));
        }

        if (importJob.WorkspaceId.Value != request.WorkspaceId)
        {
            return DomainResult.Failure<GetImportByIdResponse>(
                ImportJobApplicationErrors.NotFoundInWorkspace(
                    request.ImportJobId,
                    request.WorkspaceId));
        }

        IngestionJob? latestJob = await jobQueue.GetLatestAsync(importJob.Id, cancellationToken);
        bool isTerminal = importJob.Status.Equals(ImportStatusType.Succeeded)
            || importJob.Status.Equals(ImportStatusType.Failed)
            || importJob.Status.Equals(ImportStatusType.Cancelled);

        GetImportByIdResponse response = mapper.Map<GetImportByIdResponse>(
            new GetImportByIdMappingSource(importJob, latestJob, isTerminal));

        return DomainResult.Success(response);
    }
}