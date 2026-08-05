using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class WorkspaceRepository(EspadaDbContext dbContext) : IWorkspaceRepository
    {
        public async Task AddAsync(Workspace workspace, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspace);

            await dbContext.Workspaces.AddAsync(workspace, cancellationToken);
        }

        public async Task<Workspace?> GetByIdAsync(WorkspaceId workspaceId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);

            return await dbContext.Workspaces.FindAsync([workspaceId], cancellationToken);
        }
    }
}