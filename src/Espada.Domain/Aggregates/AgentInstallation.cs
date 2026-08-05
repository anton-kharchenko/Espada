using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class AgentInstallation : AggregateRoot<AgentInstallationId>, IHasConcurrencyVersion
    {
        public const int ExecutablePathMaxLength = 2048;

        private AgentInstallation()
        {
        }

        private AgentInstallation(AgentInstallationId id, DeviceId deviceId, AgentVendorType vendor,
            string executablePath, string? detectedVersion, bool isAuthenticated, DateTimeOffset detectedAtUtc)
            : base(id)
        {
            DeviceId = deviceId;
            Vendor = vendor;
            ExecutablePath = executablePath;
            DetectedVersion = detectedVersion;
            IsAuthenticated = isAuthenticated;
            DetectedAtUtc = detectedAtUtc;
        }

        public DeviceId DeviceId { get; private set; } = null!;
        public AgentVendorType Vendor { get; private set; } = null!;
        public string ExecutablePath { get; private set; } = string.Empty;
        public string? DetectedVersion { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public DateTimeOffset DetectedAtUtc { get; private set; }
        public uint Version { get; private set; }

        public static DomainResult<AgentInstallation> Create(AgentInstallationId id, DeviceId deviceId,
            AgentVendorType vendor, string? executablePath, string? detectedVersion, bool isAuthenticated,
            DateTimeOffset detectedAtUtc)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(deviceId);
            ArgumentNullException.ThrowIfNull(vendor);
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                return DomainResult<AgentInstallation>.Failure(AgentInstallationErrors.ExecutablePathEmpty);
            }

            string normalizedPath = executablePath.Trim();
            return normalizedPath.Length > ExecutablePathMaxLength
                ? DomainResult<AgentInstallation>.Failure(AgentInstallationErrors.ExecutablePathTooLong)
                : DomainResult<AgentInstallation>.Success(new AgentInstallation(id, deviceId, vendor, normalizedPath,
                    detectedVersion, isAuthenticated, detectedAtUtc));
        }
    }
}
