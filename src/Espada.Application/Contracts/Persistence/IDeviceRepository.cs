using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IDeviceRepository
    {
        Task AddAsync(Device device, CancellationToken cancellationToken = default);
        Task<Device?> GetByIdAsync(DeviceId deviceId, CancellationToken cancellationToken = default);
    }
}