using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Agents
{
    public interface IAgentApprovalGateway
    {
        Task<bool> RequestAsync(AgentSessionId sessionId, string toolName, string argumentsJson,
            CancellationToken cancellationToken = default);
    }
}