using Espada.DeploymentKit.Enums;

namespace Espada.DeploymentKit.Settings;

public sealed record DeploymentSettings(
    DeploymentEnvironmentType EnvironmentType,
    DeploymentTargetType TargetType,
    string Location,
    string SubscriptionId,
    string TenantId,
    string? ApiKey,
    string ImageTag,
    string RepositoryRoot,
    string? BackendUrl)
{
    public bool DeployWorkloads { get; init; } = true;

    public bool ApiEnabled { get; init; } = true;

    public string EnvironmentName => EnvironmentType.ToString().ToLowerInvariant();

    public string StackName => EnvironmentName;
}