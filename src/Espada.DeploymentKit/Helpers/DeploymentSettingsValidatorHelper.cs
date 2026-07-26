using Espada.DeploymentKit.Settings;

namespace Espada.DeploymentKit.Helpers;

public static class DeploymentSettingsValidatorHelper
{
    public static void Validate(DeploymentSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Location);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.SubscriptionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.TenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ApiKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ImageTag);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.RepositoryRoot);

        if (!Guid.TryParse(settings.SubscriptionId, out _))
        {
            throw new ArgumentException("Subscription ID must be a GUID.", nameof(settings));
        }

        if (!Guid.TryParse(settings.TenantId, out _))
        {
            throw new ArgumentException("Tenant ID must be a GUID.", nameof(settings));
        }

        if (!Directory.Exists(settings.RepositoryRoot) || !File.Exists(Path.Combine(settings.RepositoryRoot, "Espada.sln")))
        {
            throw new ArgumentException("Repository root must contain Espada.sln.", nameof(settings));
        }
    }
}