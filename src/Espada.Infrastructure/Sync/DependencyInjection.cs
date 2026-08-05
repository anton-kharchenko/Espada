using Espada.Infrastructure.Sync.Authentication;
using Espada.Infrastructure.Sync.Client;
using Espada.Infrastructure.Sync.Contracts;
using Espada.Infrastructure.Sync.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Infrastructure.Sync
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSyncInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions<LocalIdentityOptions>()
                .Bind(configuration.GetSection(LocalIdentityOptions.SectionName));
            services.AddOptions<SyncClientOptions>()
                .Bind(configuration.GetSection(SyncClientOptions.SectionName))
                .Validate(options => options.IsValid(),
                    "Sync client settings must be complete HTTPS URLs with valid polling limits.")
                .ValidateOnStart();
            services.AddSingleton<SyncTokenStore>();
            services.AddSingleton<ISyncAuthorizationService, SyncAuthorizationService>();
            services.AddSingleton<LocalSyncStateStore>();
            services.AddScoped<SyncEventApplier>();
            services.AddScoped<ISyncClientService, SyncClientService>();

            return services;
        }
    }
}