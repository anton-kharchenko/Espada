using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Espada.Infrastructure.Constants;

namespace Espada.Infrastructure.Extensions
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static void ConfigureInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            string environmentVariable =
                Environment.GetEnvironmentVariable(DatabaseConfigurationConstants.ConnectionStringEnvironmentVariable)
                ?? configuration.GetConnectionString(DatabaseConfigurationConstants.ConnectionString)
                ?? throw new InvalidOperationException(
                    $"Database connection string was not configured. Set ConnectionStrings:{DatabaseConfigurationConstants.ConnectionString} or {DatabaseConfigurationConstants.ConnectionStringEnvironmentVariable}.");
            services.AddInfrastructure(environmentVariable, configuration);
        }
    }
}