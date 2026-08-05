using Espada.Protocol.Sync.Options;
using Espada.Protocol.Sync.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Protocol.Sync
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSyncProtocol(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions<SyncServerOptions>()
                .Bind(configuration.GetSection(SyncServerOptions.SectionName))
                .Validate(options => options.IsValid(),
                    "Sync server limits must be positive when sync endpoints are enabled.")
                .ValidateOnStart();
            services.AddScoped<SyncServerService>();

            return services;
        }
    }
}