using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class AgentInstallationRepository(EspadaDbContext dbContext) : IAgentInstallationRepository
    {
        public async Task AddAsync(AgentInstallation installation, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(installation);
            await dbContext.AgentInstallations.AddAsync(installation, cancellationToken);
        }

        public async Task<IReadOnlyList<AgentInstallation>> ListByDeviceIdAsync(DeviceId deviceId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(deviceId);
            return await dbContext.AgentInstallations.AsNoTracking()
                .Where(installation => installation.DeviceId == deviceId)
                .OrderBy(installation => installation.Vendor.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
