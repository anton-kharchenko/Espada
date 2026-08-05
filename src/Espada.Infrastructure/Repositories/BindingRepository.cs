using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class BindingRepository(
        EspadaDbContext dbContext) : IBindingRepository
    {
        public async Task UpsertAsync(
            Binding binding,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(binding);
            Binding? existing = await dbContext.Bindings.FindAsync([binding.Id], cancellationToken);
            if (existing is null)
            {
                await dbContext.Bindings.AddAsync(binding, cancellationToken);
                return;
            }

            dbContext.Entry(existing).CurrentValues.SetValues(binding);
        }

        public async Task<Binding?> GetByIdAsync(
            BindingId bindingId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(bindingId);
            return await dbContext.Bindings.FindAsync([bindingId], cancellationToken);
        }

        public async Task<IReadOnlyList<Binding>> ListByWorkspaceIdAsync(
            WorkspaceId workspaceId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);
            return await dbContext.Bindings
                .AsNoTracking()
                .Where(binding => binding.WorkspaceId == workspaceId)
                .OrderBy(binding => binding.ArtifactRevisionId)
                .ThenBy(binding => binding.Id)
                .ToListAsync(cancellationToken);
        }

        public void Remove(Binding binding)
        {
            ArgumentNullException.ThrowIfNull(binding);
            dbContext.Bindings.Remove(binding);
        }
    }
}