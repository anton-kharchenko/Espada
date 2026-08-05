using Espada.Application.Models.Agents;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Agents
{
    public interface IAgentSessionExecutionQueue
    {
        ValueTask QueueAsync(AgentSessionExecution execution, CancellationToken cancellationToken = default);
        Task<bool> DecideApprovalAsync(AgentApprovalId approvalId, bool approved,
            CancellationToken cancellationToken = default);
        Task<bool> CancelAsync(AgentSessionId sessionId, CancellationToken cancellationToken = default);
    }
}