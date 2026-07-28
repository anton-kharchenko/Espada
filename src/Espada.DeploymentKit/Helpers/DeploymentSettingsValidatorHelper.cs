using Espada.DeploymentKit.Enums;
using Espada.DeploymentKit.Settings;

namespace Espada.DeploymentKit.Helpers
{
    public static class DeploymentSettingsValidatorHelper
    {
        public static void Validate(DeploymentSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentException.ThrowIfNullOrWhiteSpace(settings.Location);
            ArgumentException.ThrowIfNullOrWhiteSpace(settings.SubscriptionId);
            ArgumentException.ThrowIfNullOrWhiteSpace(settings.TenantId);
            if (!Enum.IsDefined(settings.TargetType))
            {
                throw new ArgumentOutOfRangeException(nameof(settings));
            }

            if (settings.TargetType == DeploymentTargetType.All)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(settings.ApiKey);
                ArgumentException.ThrowIfNullOrWhiteSpace(settings.ImageTag);
                ArgumentException.ThrowIfNullOrWhiteSpace(
                    settings.McpEntraAuthority);
                ArgumentException.ThrowIfNullOrWhiteSpace(
                    settings.McpEntraClientId);
                ArgumentException.ThrowIfNullOrWhiteSpace(
                    settings.McpEntraClientSecret);
                ArgumentException.ThrowIfNullOrWhiteSpace(
                    settings.McpSigningCertificate);
                ArgumentException.ThrowIfNullOrWhiteSpace(
                    settings.McpEncryptionCertificate);
            }

            if (settings.TargetType == DeploymentTargetType.Website
                && settings.EnvironmentType != DeploymentEnvironmentType.Production)
            {
                throw new ArgumentException("The website target is only available for production.", nameof(settings));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(settings.RepositoryRoot);

            if (!Guid.TryParse(settings.SubscriptionId, out _))
            {
                throw new ArgumentException("Subscription ID must be a GUID.", nameof(settings));
            }

            if (!Guid.TryParse(settings.TenantId, out _))
            {
                throw new ArgumentException("Tenant ID must be a GUID.", nameof(settings));
            }

            if (!Directory.Exists(settings.RepositoryRoot) ||
                !File.Exists(Path.Join(settings.RepositoryRoot, "Espada.sln")))
            {
                throw new ArgumentException("Repository root must contain Espada.sln.", nameof(settings));
            }
        }
    }
}
