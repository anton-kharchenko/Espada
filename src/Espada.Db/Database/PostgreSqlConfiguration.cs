using Microsoft.Extensions.Configuration;

namespace Espada.Db.Database;

internal static class PostgreSqlConfiguration
{
    public static string GetRequiredConnectionString(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return Environment.GetEnvironmentVariable(DatabaseConfigurationNames.ConnectionStringEnvironmentVariable)
            ?? configuration.GetConnectionString(DatabaseConfigurationNames.ConnectionString)
            ?? throw new InvalidOperationException($"Database connection string was not configured. Set ConnectionStrings:{DatabaseConfigurationNames.ConnectionString} or {DatabaseConfigurationNames.ConnectionStringEnvironmentVariable}.");
    }
}
