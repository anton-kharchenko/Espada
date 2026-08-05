using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface ISyncDeviceRegistrationRepository
    {
        Task<bool> IsOwnedByAsync(DeviceId deviceId, string issuer, string subject,
            CancellationToken cancellationToken = default);
        Task<int> CountByOwnerAsync(string issuer, string subject, CancellationToken cancellationToken = default);
        Task RegisterAsync(DeviceId deviceId, string name, string issuer, string subject,
            DateTimeOffset registeredAtUtc, CancellationToken cancellationToken = default);
    }
}