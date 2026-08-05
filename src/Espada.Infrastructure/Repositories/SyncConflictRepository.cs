using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Infrastructure.Database;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class SyncConflictRepository(EspadaDbContext dbContext) : ISyncConflictRepository
    {
        public async Task AddAsync(SyncConflict conflict, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(conflict);
            await dbContext.SyncConflicts.AddAsync(conflict, cancellationToken);
        }
    }
}