using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class AgentSessionRepository(EspadaDbContext dbContext) : IAgentSessionRepository
    {
        public async Task AddAsync(AgentSession session, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(session);
            await dbContext.AgentSessions.AddAsync(session, cancellationToken);
        }

        public async Task<AgentSession?> GetByIdAsync(AgentSessionId sessionId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sessionId);
            return await dbContext.AgentSessions.FindAsync([sessionId], cancellationToken);
        }

        public async Task<IReadOnlyList<AgentSession>> ListByWorkspaceIdAsync(WorkspaceId workspaceId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);
            return await dbContext.AgentSessions.AsNoTracking()
                .Where(session => session.WorkspaceId == workspaceId)
                .OrderByDescending(session => session.CreatedAtUtc)
                .ToArrayAsync(cancellationToken);
        }
    }
}