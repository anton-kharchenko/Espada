using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Imports.Queries.ListImports
{
    internal sealed class ListImportsQueryHandler(
        IWorkspaceRepository workspaceRepository,
        IImportJobRepository importJobRepository,
        IMapper mapper)
        : IQueryHandler<ListImportsQuery, ListImportsResponse>
    {
        public async Task<DomainResult<ListImportsResponse>> Handle(
            ListImportsQuery request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure<ListImportsResponse>(
                    WorkspaceApplicationErrors.InvalidId);
            }

            WorkspaceId workspaceId =
                WorkspaceId.Create(request.WorkspaceId);
            Workspace? workspace = await workspaceRepository.GetByIdAsync(
                workspaceId,
                cancellationToken);
            if (workspace is null)
            {
                return DomainResult.Failure<ListImportsResponse>(
                    WorkspaceApplicationErrors.NotFound(
                        request.WorkspaceId));
            }

            IReadOnlyList<ImportJob> imports =
                await importJobRepository.ListByWorkspaceIdAsync(
                    workspaceId,
                    cancellationToken);
            ImportListItemResponse[] items =
                mapper.Map<ImportListItemResponse[]>(imports);

            return DomainResult.Success(
                new ListImportsResponse(items));
        }
    }
}