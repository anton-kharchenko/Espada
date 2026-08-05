using Espada.Domain.Entities;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IAgentSessionEventRepository
    {
        Task AddAsync(AgentSessionEvent sessionEvent, CancellationToken cancellationToken = default);
        Task<long> GetNextSequenceAsync(AgentSessionId sessionId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AgentSessionEvent>> ListBySessionIdAsync(AgentSessionId sessionId,
            long afterSequence = 0, CancellationToken cancellationToken = default);
    }
}