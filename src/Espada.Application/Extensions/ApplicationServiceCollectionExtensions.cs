using Microsoft.Extensions.DependencyInjection;

namespace Espada.Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection ConfigureApplicationLayer(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddApplication();
    }
}