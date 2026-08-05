using Espada.LocalSetup.Contracts;
using Espada.LocalSetup.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.LocalSetup
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddLocalSetup(this IServiceCollection services)
        {
            services.AddSingleton<GitRepositoryInspector>();
            services.AddSingleton<IAgentDiscoveryService, AgentDiscoveryService>();
            services.AddSingleton<McpConfigurationPreviewService>();
            services.AddSingleton<ManagedMcpConfigurationWriter>();
            services.AddSingleton<LocalRuntimeConfigurationWriter>();
            services.AddScoped<ILocalSetupService, LocalSetupService>();

            return services;
        }
    }
}