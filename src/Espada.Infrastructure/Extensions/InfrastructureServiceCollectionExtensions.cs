using Espada.Infrastructure.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static void ConfigureInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string environmentVariable = Environment.GetEnvironmentVariable(DatabaseConfigurationNames.ConnectionStringEnvironmentVariable)
                                     ?? configuration.GetConnectionString(DatabaseConfigurationNames.ConnectionString)
                                     ?? throw new InvalidOperationException($"Database connection string was not configured. Set ConnectionStrings:{DatabaseConfigurationNames.ConnectionString} or {DatabaseConfigurationNames.ConnectionStringEnvironmentVariable}.");
        services.AddInfrastructure(environmentVariable, configuration);
    }
}