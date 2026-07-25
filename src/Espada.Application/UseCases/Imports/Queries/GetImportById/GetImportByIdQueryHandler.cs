using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Imports.Queries.GetImportById
{
    internal sealed class GetImportByIdQueryHandler(IImportJobRepository importJobRepository)
        : IQueryHandler<GetImportByIdQuery, GetImportByIdResponse>
    {
        public async Task<DomainResult<GetImportByIdResponse>> Handle(GetImportByIdQuery request, CancellationToken cancellationToken)
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
                return DomainResult.Failure<GetImportByIdResponse>(ImportJobApplicationErrors.NotFound(request.ImportJobId));
            }

            if (importJob.WorkspaceId.Value != request.WorkspaceId)
            {
                return DomainResult.Failure<GetImportByIdResponse>(ImportJobApplicationErrors.NotFoundInWorkspace(request.ImportJobId, request.WorkspaceId));
            }

            GetImportByIdResponse response = new(
                importJob.Id.Value,
                importJob.SourceId.Value,
                importJob.WorkspaceId.Value,
                importJob.Status.Id,
                importJob.Status.Name,
                importJob.RequestedAtUtc,
                importJob.StartedAtUtc,
                importJob.CompletedAtUtc,
                importJob.ArtifactId?.Value,
                importJob.ArtifactRevisionId?.Value,
                importJob.Failure?.Code,
                importJob.Failure?.Reason);

            return DomainResult.Success(response);
        }
    }
}