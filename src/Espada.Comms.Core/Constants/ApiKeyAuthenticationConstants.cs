namespace Espada.Comms.Core.Constants
{
    public static class ApiKeyAuthenticationConstants
    {
        public const string AuthenticationScheme = "ApiKey";
        public const string DefaultHeaderName = "X-Espada-Api-Key";
        public const string ConfigurationSection = "Authentication:ApiKey";
        public const string EnvironmentVariable = "ESPADA_API_KEY";
    }
}