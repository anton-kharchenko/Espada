using Espada.Application.Extensions;
using Espada.Comms.Core.Security;
using Espada.Infrastructure.Extensions;

namespace Espada.Daemon.Extensions
{
    public static class DaemonExtensions
    {
        public static void AddEspadaDaemon(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.ConfigureApplicationLayer();
            services.ConfigureInfrastructure(configuration);
            services.AddEspadaApiKeyAuthentication(configuration);
            services.AddAuthorization();
            services.AddProblemDetails();
        }

        public static void UseEspadaDaemon(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            app.UseExceptionHandler();
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}