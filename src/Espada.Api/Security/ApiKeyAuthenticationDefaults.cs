namespace Espada.Api.Security;

internal static class ApiKeyAuthenticationDefaults
{
    public const string AuthenticationScheme = "ApiKey";
    public const string DefaultHeaderName = "X-Espada-Api-Key";
    public const string ConfigurationSection = "Authentication:ApiKey";
}