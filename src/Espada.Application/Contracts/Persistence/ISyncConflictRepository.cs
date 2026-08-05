using Espada.Domain.Aggregates;

namespace Espada.Application.Contracts.Persistence
{
    public interface ISyncConflictRepository
    {
        Task AddAsync(SyncConflict conflict, CancellationToken cancellationToken = default);
    }
}