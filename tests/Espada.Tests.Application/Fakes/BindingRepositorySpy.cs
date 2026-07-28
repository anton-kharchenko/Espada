using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class BindingRepositorySpy : IBindingRepository
    {
        public Binding? BindingToReturn { get; set; }
        public Binding? UpsertedBinding { get; private set; }
        public IReadOnlyList<Binding> BindingsToReturn { get; set; } = [];
        public bool WasRemoved { get; private set; }
        public CancellationToken UpsertCancellationToken { get; private set; }

        public Task UpsertAsync(Binding binding, CancellationToken cancellationToken = default)
        {
            UpsertedBinding = binding;
            UpsertCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }

        public Task<Binding?> GetByIdAsync(BindingId bindingId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(BindingToReturn);
        }

        public Task<IReadOnlyList<Binding>> ListByWorkspaceIdAsync(WorkspaceId workspaceId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(BindingsToReturn);
        }

        public void Remove(Binding binding)
        {
            WasRemoved = true;
        }
    }
}