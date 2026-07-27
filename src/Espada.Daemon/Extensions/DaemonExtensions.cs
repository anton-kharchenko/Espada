using Espada.Application.Extensions;
using Espada.Comms.Core.Security;
using Espada.Daemon.Mappings;
using Espada.Daemon.Services;
using Espada.Infrastructure.Extensions;
using Espada.Protocol.Mcp.Contracts.Requests;
using Espada.Protocol.Mcp.Service;

namespace Espada.Daemon.Extensions;

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
        services.AddScoped<IContextSearchToolService, LocalContextSearchToolService>();
        services.AddAutoMapper(_ => { }, typeof(DaemonMappingProfile));
        services
            .AddMcpServer()
            .WithHttpTransport(options => options.Stateless = true)
            .WithTools<ContextSearchTool>();
    }

    public static void UseEspadaDaemon(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseExceptionHandler();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapMcp("/mcp").RequireAuthorization();
        app.MapPost(
                "/internal/context/search",
                async (
                    ContextSearchRequest request,
                    IContextSearchToolService service,
                    CancellationToken cancellationToken) =>
                    Results.Ok(await service.SearchAsync(request, cancellationToken)))
            .RequireAuthorization();
    }
}