using Espada.Application.Contracts.Persistence;
using Espada.Db.Models;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class SyncDeviceRegistrationRepository(EspadaDbContext dbContext)
        : ISyncDeviceRegistrationRepository
    {
        public Task<bool> IsOwnedByAsync(DeviceId deviceId, string issuer, string subject,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(deviceId);
            return dbContext.SyncDeviceRegistrations.AsNoTracking()
                .AnyAsync(registration => registration.DeviceId == deviceId.Value
                                          && registration.Issuer == issuer
                                          && registration.Subject == subject, cancellationToken);
        }

        public Task<int> CountByOwnerAsync(string issuer, string subject,
            CancellationToken cancellationToken = default)
        {
            return dbContext.SyncDeviceRegistrations.AsNoTracking()
                .CountAsync(registration => registration.Issuer == issuer
                                            && registration.Subject == subject, cancellationToken);
        }

        public async Task RegisterAsync(DeviceId deviceId, string name, string issuer, string subject,
            DateTimeOffset registeredAtUtc, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(deviceId);
            SyncDeviceRegistrations? existing = await dbContext.SyncDeviceRegistrations
                .SingleOrDefaultAsync(registration => registration.DeviceId == deviceId.Value, cancellationToken);
            if (existing is not null)
            {
                if (existing.Issuer != issuer || existing.Subject != subject)
                {
                    throw new InvalidOperationException("The sync device is registered to another identity.");
                }

                return;
            }

            Device? device = await dbContext.Devices.FindAsync([deviceId], cancellationToken);
            if (device is null)
            {
                device = Device.Create(deviceId, name, registeredAtUtc).Value;
                await dbContext.Devices.AddAsync(device, cancellationToken);
            }

            await dbContext.SyncDeviceRegistrations.AddAsync(
                new SyncDeviceRegistrations(deviceId.Value, issuer, subject, registeredAtUtc), cancellationToken);
        }
    }
}