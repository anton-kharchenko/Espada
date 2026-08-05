using Espada.AgentAdapters.Execution;
using Espada.AgentAdapters.Git;
using Espada.AgentAdapters.Processes;
using Espada.Application.Contracts.Agents;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.AgentAdapters
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAgentAdapters(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddSingleton<IAgentProcessClient, CodexAppServerClient>();
            services.AddSingleton<IAgentProcessClient, ClaudeProcessClient>();
            services.AddSingleton<IAgentProcessClient, GeminiAcpClient>();
            services.AddSingleton<IAgentProcessClient, GrokAcpClient>();
            services.AddSingleton<IAgentWorktreeService, AgentWorktreeService>();
            services.AddSingleton<AgentSessionExecutionService>();
            services.AddSingleton<IAgentSessionExecutionQueue>(serviceProvider =>
                serviceProvider.GetRequiredService<AgentSessionExecutionService>());
            services.AddHostedService(serviceProvider =>
                serviceProvider.GetRequiredService<AgentSessionExecutionService>());
            return services;
        }
    }
}