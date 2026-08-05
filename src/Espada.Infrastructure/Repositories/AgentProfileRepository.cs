using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class AgentProfileRepository(EspadaDbContext dbContext) : IAgentProfileRepository
    {
        public async Task AddAsync(AgentProfile profile, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(profile);
            await dbContext.AgentProfiles.AddAsync(profile, cancellationToken);
        }

        public async Task<AgentProfile?> GetByIdAsync(AgentProfileId profileId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(profileId);
            return await dbContext.AgentProfiles.FindAsync([profileId], cancellationToken);
        }
        public async Task<IReadOnlyList<AgentProfile>> ListByWorkspaceIdAsync(WorkspaceId workspaceId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(workspaceId);
            return await dbContext.AgentProfiles.AsNoTracking()
                .Where(profile => profile.WorkspaceId == workspaceId)
                .OrderBy(profile => profile.Vendor.Id)
                .ThenBy(profile => profile.Name)
                .ToListAsync(cancellationToken);
        }
    }
}