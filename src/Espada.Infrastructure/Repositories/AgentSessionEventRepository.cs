using Espada.Application.Contracts.Persistence;
using Espada.Domain.Entities;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class AgentSessionEventRepository(EspadaDbContext dbContext) : IAgentSessionEventRepository
    {
        public async Task AddAsync(AgentSessionEvent sessionEvent, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sessionEvent);
            await dbContext.AgentSessionEvents.AddAsync(sessionEvent, cancellationToken);
        }

        public async Task<long> GetNextSequenceAsync(AgentSessionId sessionId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sessionId);
            long? lastSequence = await dbContext.AgentSessionEvents
                .Where(sessionEvent => sessionEvent.AgentSessionId == sessionId)
                .MaxAsync(sessionEvent => (long?)sessionEvent.Sequence, cancellationToken);
            return lastSequence.GetValueOrDefault() + 1;
        }

        public async Task<IReadOnlyList<AgentSessionEvent>> ListBySessionIdAsync(AgentSessionId sessionId,
            long afterSequence = 0, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sessionId);
            return await dbContext.AgentSessionEvents.AsNoTracking()
                .Where(sessionEvent => sessionEvent.AgentSessionId == sessionId
                                       && sessionEvent.Sequence > afterSequence)
                .OrderBy(sessionEvent => sessionEvent.Sequence)
                .ToArrayAsync(cancellationToken);
        }
    }
}