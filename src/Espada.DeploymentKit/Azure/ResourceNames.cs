using Espada.DeploymentKit.Constants;
using Espada.DeploymentKit.Enums;

namespace Espada.DeploymentKit.Azure
{
    internal sealed record ResourceNames(
        string ResourceGroup,
        string Registry,
        string LogAnalytics,
        string ApplicationInsights,
        string ContainerEnvironment,
        string ApiIdentity,
        string McpIdentity,
        string WorkerIdentity,
        string MigrationIdentity,
        string StorageAccount,
        string KeyVault,
        string PostgreSqlServer,
        string PostgreSqlDatabase,
        string Api,
        string Mcp,
        string Worker,
        string MigrationJob)
    {
        public static ResourceNames Create(
            DeploymentEnvironmentType environmentType,
            string subscriptionId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionId);

            string suffix = environmentType switch
            {
                DeploymentEnvironmentType.Development => "dev",
                DeploymentEnvironmentType.Staging => "stg",
                DeploymentEnvironmentType.Production => "prod",
                _ => throw new ArgumentOutOfRangeException(nameof(environmentType))
            };
            string uniqueSuffix = subscriptionId.Replace("-", string.Empty, StringComparison.Ordinal)[..8];

            return new ResourceNames(
                $"espada-{suffix}",
                $"espada{suffix}{uniqueSuffix}",
                $"espada-{suffix}-logs",
                $"espada-{suffix}-insights",
                $"espada-{suffix}-cae",
                $"espada-{suffix}-api-id",
                $"espada-{suffix}-mcp-id",
                $"espada-{suffix}-worker-id",
                $"espada-{suffix}-db-id",
                $"espada{suffix}{uniqueSuffix}blob",
                $"espada-{suffix}-{uniqueSuffix}-kv",
                $"espada-{suffix}-{uniqueSuffix}-pg",
                DatabaseConfigurationConstants.DatabaseName,
                $"espada-{suffix}-api",
                $"espada-{suffix}-mcp",
                $"espada-{suffix}-worker",
                $"espada-{suffix}-migrate");
        }
    }
}