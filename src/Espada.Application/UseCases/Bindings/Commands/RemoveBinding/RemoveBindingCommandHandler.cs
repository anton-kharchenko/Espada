using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Bindings.Commands.RemoveBinding
{
    internal sealed class RemoveBindingCommandHandler(
        IBindingRepository bindingRepository,
        IUnitOfWork unitOfWork)
        : ICommandHandler<RemoveBindingCommand>
    {
        public async Task<DomainResult> Handle(
            RemoveBindingCommand request,
            CancellationToken cancellationToken)
        {
            if (request.WorkspaceId == Guid.Empty || request.BindingId == Guid.Empty)
            {
                return DomainResult.Failure(
                    request.WorkspaceId == Guid.Empty
                        ? WorkspaceApplicationErrors.InvalidId
                        : BindingApplicationErrors.InvalidId);
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
                return DomainResult.Failure(error);
            }

            bindingRepository.Remove(binding);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return DomainResult.Success();
        }
    }
}