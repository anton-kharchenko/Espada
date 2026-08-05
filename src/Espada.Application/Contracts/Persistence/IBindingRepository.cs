using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IBindingRepository
    {
        Task UpsertAsync(
            Binding binding,
            CancellationToken cancellationToken = default);

        Task<Binding?> GetByIdAsync(
            BindingId bindingId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Binding>> ListByWorkspaceIdAsync(
            WorkspaceId workspaceId,
            CancellationToken cancellationToken = default);

        void Remove(Binding binding);
    }
}