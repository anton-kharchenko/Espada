using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.UseCases.Bindings.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Bindings.Queries.GetBinding
{
    internal sealed class GetBindingQueryHandler(
        IBindingRepository bindingRepository,
        IMapper mapper)
        : IQueryHandler<GetBindingQuery, BindingResponse>
    {
        public async Task<DomainResult<BindingResponse>> Handle(
            GetBindingQuery request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty || request.BindingId == Guid.Empty)
            {
                DomainError error = request.WorkspaceId == Guid.Empty
                    ? WorkspaceApplicationErrors.InvalidId
                    : BindingApplicationErrors.InvalidId;
                return DomainResult.Failure<BindingResponse>(error);
            }

            Binding? binding = await bindingRepository.GetByIdAsync(
                BindingId.Create(request.BindingId),
                cancellationToken);
            if (binding is null || binding.WorkspaceId.Value != request.WorkspaceId)
            {
                DomainError error = binding is null
                    ? BindingApplicationErrors.NotFound(request.BindingId)
                    : BindingApplicationErrors.NotFoundInWorkspace(
                        request.BindingId,
                        request.WorkspaceId);
                return DomainResult.Failure<BindingResponse>(error);
            }

            return DomainResult.Success(mapper.Map<BindingResponse>(binding));
        }
    }
}