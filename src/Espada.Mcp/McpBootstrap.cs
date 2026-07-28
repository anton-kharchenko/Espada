using Espada.Application.Contracts.Security;
using Espada.Application.Extensions;
using Espada.Infrastructure.Extensions;
using Espada.Mcp.Constants;
using Espada.Mcp.Security;
using Espada.Protocol.Mcp.Extensions;
using Espada.Protocol.Mcp.Resources;
using Espada.Protocol.Mcp.Tools;
using Espada.ServiceDefaults.Extensions;

namespace Espada.Mcp
{
    public static class McpBootstrap
    {
        private const string StdioTransport = "stdio";
        private const string HttpTransport = "http";

        public static Task<int> RunAsync(
            string[] args,
            CancellationToken cancellationToken = default)
        {
            string? requestedTransport =
                args.FirstOrDefault()?.Trim().ToLowerInvariant();
            bool hasExplicitTransport =
                requestedTransport is StdioTransport or HttpTransport;
            string transport = hasExplicitTransport
                ? requestedTransport!
                : HttpTransport;
            string[] hostArgs = hasExplicitTransport
                ? args.Skip(1).ToArray()
                : args;

            return transport switch
            {
                StdioTransport => RunStdioAsync(hostArgs, cancellationToken),
                HttpTransport => RunHttpAsync(hostArgs, cancellationToken),
                _ => throw new InvalidOperationException(
                    "Unsupported MCP transport.")
            };
        }

        public static async Task<int> RunStdioAsync(
            string[] args,
            CancellationToken cancellationToken = default)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
            builder.AddServiceDefaults();
            builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
            AddCoreServices(builder.Services, builder.Configuration);
            builder.Services.AddSingleton<IRequestPrincipalAccessor,
                TrustedLocalRequestPrincipalAccessor>();
            builder.Services
                .AddMcpServer()
                .WithStdioServerTransport()
                .WithTools<WorkspaceTools>()
                .WithTools<MemoryTools>()
                .WithTools<SourceTools>()
                .WithTools<ArtifactTools>()
                .WithTools<BindingTools>()
                .WithTools<ContextTools>()
                .WithResources<WorkspaceResources>()
                .WithResources<ArtifactResources>();

            await builder.Build().RunAsync(cancellationToken);
            return 0;
        }

        public static async Task<int> RunHttpAsync(
            string[] args,
            CancellationToken cancellationToken = default)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            if (string.IsNullOrWhiteSpace(builder.Configuration["urls"]))
            {
                builder.WebHost.UseUrls("http://127.0.0.1:7433");
            }

            builder.AddServiceDefaults();
            AddCoreServices(builder.Services, builder.Configuration);
            builder.Services.AddMcpAuthorization(builder.Configuration);
            builder.Services.AddScoped<IRequestPrincipalAccessor,
                HttpRequestPrincipalAccessor>();
            builder.Services
                .AddMcpServer()
                .WithHttpTransport(options => options.Stateless = true)
                .WithTools<WorkspaceTools>()
                .WithTools<MemoryTools>()
                .WithTools<SourceTools>()
                .WithTools<ArtifactTools>()
                .WithTools<BindingTools>()
                .WithTools<ContextTools>()
                .WithResources<WorkspaceResources>()
                .WithResources<ArtifactResources>();

            WebApplication app = builder.Build();
            if (app.Environment.IsEnvironment("Testing"))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler();
            }

            app.UseMiddleware<McpOriginValidationMiddleware>();
            app.UseMiddleware<RefreshTokenReuseMiddleware>();
            app.UseAuthentication();
            app.UseMiddleware<McpChallengeMetadataMiddleware>();
            app.UseAuthorization();
            app.UseRateLimiter();
            app.MapMcpAuthorizationEndpoints();
            app.MapMcp("/mcp")
                .RequireAuthorization(McpAuthorizationConstants.AccessPolicy)
                .RequireRateLimiting(
                    McpAuthorizationConstants.RateLimitPolicy);
            app.MapDefaultEndpoints();

            await app.RunAsync(cancellationToken);
            return 0;
        }

        private static void AddCoreServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.ConfigureApplicationLayer();
            services.ConfigureInfrastructure(configuration);
            services.AddEspadaMcpProtocol();
            services.AddProblemDetails();
        }
    }
}