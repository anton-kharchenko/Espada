using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IAgentApprovalRepository
    {
        Task AddAsync(AgentApproval approval, CancellationToken cancellationToken = default);
        Task<AgentApproval?> GetByIdAsync(AgentApprovalId approvalId,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AgentApproval>> ListBySessionIdAsync(AgentSessionId sessionId,
            CancellationToken cancellationToken = default);
    }
}