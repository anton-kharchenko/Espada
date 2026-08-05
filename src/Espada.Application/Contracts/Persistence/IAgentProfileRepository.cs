using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IAgentProfileRepository
    {
        Task AddAsync(AgentProfile profile, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AgentProfile>> ListByWorkspaceIdAsync(WorkspaceId workspaceId,
            CancellationToken cancellationToken = default);
    }
}
