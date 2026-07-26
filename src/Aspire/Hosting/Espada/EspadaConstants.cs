namespace Aspire.Hosting.Espada;

internal static class EspadaConstants
{
    public static class Ports
    {
        public const int Postgres = 5433;
    }

    public static class ParameterNames
    {
        public const string PostgresPassword = "postgres-password";
        public const string ApiKey = "api-key";
    }

    public static class ParameterDefaults
    {
        public const string PostgresPassword = "postgres";
    }

    public static class ConfigurationKeys
    {
        public const string AspNetCoreEnvironment = "ASPNETCORE_ENVIRONMENT";
        public const string DotNetEnvironment = "DOTNET_ENVIRONMENT";
        public const string ParametersSectionPrefix = "Parameters:";
        public const string ApiKey = "Authentication__ApiKey__Value";
    }

    public static class ConfigurationValues
    {
        public const string Development = "Development";
    }
}