using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class DeviceRepository(EspadaDbContext dbContext) : IDeviceRepository
    {
        public async Task AddAsync(Device device, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(device);
            await dbContext.Devices.AddAsync(device, cancellationToken);
        }

        public async Task<Device?> GetByIdAsync(DeviceId deviceId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(deviceId);
            return await dbContext.Devices.FindAsync([deviceId], cancellationToken);
        }
    }
}
