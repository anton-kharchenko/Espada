using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IAgentInstallationRepository
    {
        Task AddAsync(AgentInstallation installation, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AgentInstallation>> ListByDeviceIdAsync(DeviceId deviceId,
            CancellationToken cancellationToken = default);
    }
}
