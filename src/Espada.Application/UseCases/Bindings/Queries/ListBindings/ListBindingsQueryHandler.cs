using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.UseCases.Bindings.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Bindings.Queries.ListBindings
{
    internal sealed class ListBindingsQueryHandler(
        IWorkspaceRepository workspaceRepository,
        IBindingRepository bindingRepository,
        IMapper mapper)
        : IQueryHandler<ListBindingsQuery, ListBindingsResponse>
    {
        public async Task<DomainResult<ListBindingsResponse>> Handle(
            ListBindingsQuery request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty)
            {
                return DomainResult.Failure<ListBindingsResponse>(WorkspaceApplicationErrors.InvalidId);
            }

            WorkspaceId workspaceId = WorkspaceId.Create(request.WorkspaceId);
            Workspace? workspace = await workspaceRepository.GetByIdAsync(
                workspaceId,
                cancellationToken);
            if (workspace is null)
            {
                return DomainResult.Failure<ListBindingsResponse>(
                    WorkspaceApplicationErrors.NotFound(request.WorkspaceId));
            }

            IReadOnlyList<Binding> bindings = await bindingRepository.ListByWorkspaceIdAsync(
                workspaceId,
                cancellationToken);
            BindingResponse[] items = mapper.Map<BindingResponse[]>(bindings);

            return DomainResult.Success(new ListBindingsResponse(items));
        }
    }
}