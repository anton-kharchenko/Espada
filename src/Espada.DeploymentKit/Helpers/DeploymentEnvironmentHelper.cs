using Espada.DeploymentKit.Enums;

namespace Espada.DeploymentKit.Helpers;

public static class DeploymentEnvironmentHelper
{
    public static DeploymentEnvironmentType Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToLowerInvariant() switch
        {
            "dev" or "development" => DeploymentEnvironmentType.Development,
            "staging" => DeploymentEnvironmentType.Staging,
            "prod" or "production" => DeploymentEnvironmentType.Production,
            _ => throw new ArgumentException("EnvironmentType must be dev, staging, or production.", nameof(value))
        };
    }
}