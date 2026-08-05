using Espada.Protocol.Mcp.Mappings;
using Espada.Protocol.Mcp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Protocol.Mcp.Extensions
{
    public static class McpServiceCollectionExtensions
    {
        public static IServiceCollection AddEspadaMcpProtocol(
            this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddAutoMapper(_ => { }, typeof(McpMappingProfile));
            services.AddScoped<McpApplicationExecutor>();

            return services;
        }
    }
}