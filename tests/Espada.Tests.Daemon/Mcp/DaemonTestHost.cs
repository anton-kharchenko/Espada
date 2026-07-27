using Espada.Protocol.Mcp.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Espada.Tests.Daemon.Mcp;

internal sealed class DaemonFactory(ContextSearchServiceStub service) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseEnvironment("Testing")
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureAppConfiguration(configuration =>
                configuration.AddJsonFile(
                    Path.Join(AppContext.BaseDirectory, "appsettings.Testing.json"),
                    optional: false));

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IContextSearchToolService>();
            services.AddSingleton<IContextSearchToolService>(service);
        });
    }
}