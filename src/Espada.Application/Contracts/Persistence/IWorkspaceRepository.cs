using Espada.Domain.Aggregates;

namespace Espada.Application.Contracts.Persistence
{
    public interface IWorkspaceRepository
    {
        Task AddAsync(Workspace workspace, CancellationToken cancellationToken = default);
    }
}