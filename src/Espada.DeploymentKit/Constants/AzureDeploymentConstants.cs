namespace Espada.DeploymentKit.Constants
{
    internal static class AzureDeploymentConstants
    {
        public const string ApplicationName = "espada";
        public const string ManagedBy = "pulumi";
        public const string WebsiteDomainName = "espada.website";
        public const string WebsiteResourceGroupName = "espada-website-prod";
        public const string WebsiteStaticSiteName = "espada-website";
        public const string WebsiteSourceDirectory = "src/Espada.Web";
        public const string WebsiteDistDirectory = "./dist";
        public const string StaticWebAppsCliPackage = "@azure/static-web-apps-cli@2.0.10";
        public const string Enabled = "Enabled";
        public const string Disabled = "Disabled";
        public const string PostgreSqlVersion = "17";
        public const string PostgreSqlAdministratorLogin = "espada_admin";
        public const int PostgreSqlPasswordLength = 32;
        public const string ApiImageRepository = "espada-api";
        public const string DatabaseImageRepository = "espada-db";
        public const string McpImageRepository = "espada-mcp";
        public const string WorkerImageRepository = "espada-worker";
        public const string ApiDockerfile = "src/Espada.Api/Dockerfile";
        public const string DatabaseDockerfile = "src/Espada.Db/Dockerfile";
        public const string McpDockerfile = "src/Espada.Mcp/Dockerfile";
        public const string WorkerDockerfile = "src/Espada.Worker/Dockerfile";
        public const string ApiKeySecret = "api-key";
        public const string McpEntraClientSecret = "mcp-entra-client-secret";
        public const string McpSigningCertificateSecret = "mcp-signing-certificate";
        public const string McpEncryptionCertificateSecret = "mcp-encryption-certificate";
        public const string AdministratorPasswordSecret = "postgres-admin-password";
        public const string AdministratorConnectionStringSecret = "postgres-admin-connection-string";
        public const string AspNetCoreHttpPortsEnvironmentVariable = "ASPNETCORE_HTTP_PORTS";
        public const string AspNetCoreForwardedHeadersEnvironmentVariable =
            "ASPNETCORE_FORWARDEDHEADERS_ENABLED";

        public const string ApplicationInsightsConnectionStringEnvironmentVariable =
            "APPLICATIONINSIGHTS_CONNECTION_STRING";

        public const string ResourceGroupOutput = "resourceGroupName";
        public const string RegistryNameOutput = "containerRegistryName";
        public const string RegistryLoginServerOutput = "containerRegistryLoginServer";
        public const string MigrationJobOutput = "migrationJobName";
        public const string ApiUrlOutput = "apiUrl";
        public const string McpUrlOutput = "mcpUrl";
        public const string WorkerOutput = "workerName";
        public const string BlobProviderEnvironmentVariable = "BlobStorage__Provider";
        public const string BlobContainerUriEnvironmentVariable = "BlobStorage__AzureContainerUri";
        public const string McpIssuerEnvironmentVariable = "Mcp__Authorization__Issuer";
        public const string McpResourceEnvironmentVariable = "Mcp__Authorization__Resource";
        public const string McpEntraAuthorityEnvironmentVariable = "Mcp__Authorization__EntraAuthority";
        public const string McpEntraClientIdEnvironmentVariable = "Mcp__Authorization__EntraClientId";
        public const string McpEntraClientSecretEnvironmentVariable = "Mcp__Authorization__EntraClientSecret";

        public const string McpSigningCertificateEnvironmentVariable =
            "Mcp__Authorization__SigningCertificateBase64";

        public const string McpEncryptionCertificateEnvironmentVariable =
            "Mcp__Authorization__EncryptionCertificateBase64";
    }
}