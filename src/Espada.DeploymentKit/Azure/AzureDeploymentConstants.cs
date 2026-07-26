namespace Espada.DeploymentKit.Azure;

internal static class AzureDeploymentConstants
{
    public const string ApplicationName = "espada";
    public const string ManagedBy = "pulumi";
    public const string Enabled = "Enabled";
    public const string Disabled = "Disabled";
    public const string PostgreSqlVersion = "17";
    public const string PostgreSqlAdministratorLogin = "espada_admin";
    public const int PostgreSqlPasswordLength = 32;
    public const string ApiImageRepository = "espada-api";
    public const string DatabaseImageRepository = "espada-db";
    public const string ApiDockerfile = "src/Espada.Api/Dockerfile";
    public const string DatabaseDockerfile = "src/Espada.Db/Dockerfile";
    public const string ApiKeySecret = "api-key";
    public const string AdministratorPasswordSecret = "postgres-admin-password";
    public const string AdministratorConnectionStringSecret = "postgres-admin-connection-string";
    public const string AspNetCoreHttpPortsEnvironmentVariable = "ASPNETCORE_HTTP_PORTS";
    public const string ApplicationInsightsConnectionStringEnvironmentVariable = "APPLICATIONINSIGHTS_CONNECTION_STRING";
    public const string ResourceGroupOutput = "resourceGroupName";
    public const string RegistryNameOutput = "containerRegistryName";
    public const string RegistryLoginServerOutput = "containerRegistryLoginServer";
    public const string MigrationJobOutput = "migrationJobName";
    public const string ApiUrlOutput = "apiUrl";
}