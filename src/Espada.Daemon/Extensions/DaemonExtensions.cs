using Espada.Daemon.Runtime;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Espada.Daemon.Extensions
{
    public static class DaemonExtensions
    {
        public static void AddEspadaDaemon(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddOptions<LocalRuntimeOptions>()
                .Bind(configuration.GetSection(LocalRuntimeOptions.SectionName))
                .Validate(options => options.StartupTimeoutSeconds > 0,
                    "Startup timeout must be positive.")
                .Validate(options => options.ShutdownTimeoutSeconds > 0,
                    "Shutdown timeout must be positive.")
                .ValidateOnStart();
            services.AddSingleton<LocalRuntimeStatus>();
            services.AddHostedService<LocalRuntimeHostedService>();
            services.AddHealthChecks().AddCheck<LocalRuntimeHealthCheck>("local-runtime");
            services.AddProblemDetails();
        }

        public static void UseEspadaDaemon(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            app.UseExceptionHandler();
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = registration => registration.Name == "local-runtime"
            });
            app.MapGet("/runtime/status", (LocalRuntimeStatus status) => Results.Ok(new
            {
                status = status.Status
            }));
            app.MapPost("/runtime/stop", (IHostApplicationLifetime lifetime) =>
            {
                lifetime.StopApplication();
                return Results.Accepted();
            });
        }
    }
}