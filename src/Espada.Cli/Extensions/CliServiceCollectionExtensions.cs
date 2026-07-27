using Espada.Cli.Daemon;
using Espada.Comms.Core.Security;
using Espada.Protocol.Mcp.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Espada.Cli.Extensions;

public static class CliServiceCollectionExtensions
{
    public static void AddEspadaMcpStdioBridge(this IServiceCollection services, DaemonConnection connection)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(connection);

        services.AddSingleton(connection);

        services.AddHttpClient<IContextSearchToolService, RemoteContextSearchToolService>((serviceProvider, client) =>
        {
            DaemonConnection daemon = serviceProvider.GetRequiredService<DaemonConnection>();
            client.BaseAddress = daemon.BaseUri;
            client.DefaultRequestHeaders.Add(ApiKeyAuthenticationDefaults.DefaultHeaderName, daemon.ApiKey);
        });

        services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithTools<ContextSearchTool>();
    }
}