using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.UseCases.Sources.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Sources.Queries.ListSources
{
    internal sealed class ListSourcesQueryHandler(
        IWorkspaceRepository workspaceRepository,
        ISourceRepository sourceRepository,
        IMapper mapper)
        : IQueryHandler<ListSourcesQuery, ListSourcesResponse>
    {
        public async Task<DomainResult<ListSourcesResponse>> Handle(
            ListSourcesQuery request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure<ListSourcesResponse>(
                    WorkspaceApplicationErrors.InvalidId);
            }

            WorkspaceId workspaceId =
                WorkspaceId.Create(request.WorkspaceId);
            Workspace? workspace = await workspaceRepository.GetByIdAsync(
                workspaceId,
                cancellationToken);
            if (workspace is null)
            {
                return DomainResult.Failure<ListSourcesResponse>(
                    WorkspaceApplicationErrors.NotFound(
                        request.WorkspaceId));
            }

            IReadOnlyList<Source> sources =
                await sourceRepository.ListByWorkspaceIdAsync(
                    workspaceId,
                    cancellationToken);
            SourceResponse[] items =
                mapper.Map<SourceResponse[]>(sources);

            return DomainResult.Success(
                new ListSourcesResponse(items));
        }
    }
}