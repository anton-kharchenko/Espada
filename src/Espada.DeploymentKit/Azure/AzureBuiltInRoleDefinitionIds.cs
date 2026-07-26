namespace Espada.DeploymentKit.Azure;

internal static class AzureBuiltInRoleDefinitionIds
{
    // Azure role assignments require the stable role definition ID, not the display name.
    public const string AcrPull = "7f951dda-4ed3-4680-a7ca-43fe172d538d";
    public const string KeyVaultAdministrator = "00482a5a-887f-4fb3-b363-3b7fe8e74483";
    public const string KeyVaultSecretsUser = "4633458b-17de-408a-b874-0445c86b69e6";
}