using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static void ConfigureInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString =
            configuration.GetConnectionString("espada")
            ?? configuration.GetConnectionString("Espada")
            ?? throw new InvalidOperationException("Connection string 'espada' or 'Espada' was not configured.");

        services.AddInfrastructure(connectionString);
    }
}