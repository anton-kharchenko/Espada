using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IAgentSessionRepository
    {
        Task AddAsync(AgentSession session, CancellationToken cancellationToken = default);
        Task<AgentSession?> GetByIdAsync(AgentSessionId sessionId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AgentSession>> ListByWorkspaceIdAsync(WorkspaceId workspaceId,
            CancellationToken cancellationToken = default);
    }
}