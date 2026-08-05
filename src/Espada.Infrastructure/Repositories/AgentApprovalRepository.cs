using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class AgentApprovalRepository(EspadaDbContext dbContext) : IAgentApprovalRepository
    {
        public async Task AddAsync(AgentApproval approval, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(approval);
            await dbContext.AgentApprovals.AddAsync(approval, cancellationToken);
        }

        public async Task<AgentApproval?> GetByIdAsync(AgentApprovalId approvalId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(approvalId);
            return await dbContext.AgentApprovals.FindAsync([approvalId], cancellationToken);
        }

        public async Task<IReadOnlyList<AgentApproval>> ListBySessionIdAsync(AgentSessionId sessionId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sessionId);
            return await dbContext.AgentApprovals.AsNoTracking()
                .Where(approval => approval.AgentSessionId == sessionId)
                .OrderBy(approval => approval.RequestedAtUtc)
                .ToArrayAsync(cancellationToken);
        }
    }
}