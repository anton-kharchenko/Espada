using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IWorkspaceRepository
    {
        Task AddAsync(Workspace workspace, CancellationToken cancellationToken = default);
        
        Task<Workspace?> GetByIdAsync(WorkspaceId workspaceId, CancellationToken cancellationToken = default);
    }
}