using Azure.Monitor.OpenTelemetry.AspNetCore;
using Espada.ServiceDefaults.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Espada.ServiceDefaults;

public static class Extensions
{
    public static void AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        var openTelemetry = builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics.AddRuntimeInstrumentation());

        switch (ResolveTelemetryExporter(builder.Configuration))
        {
            case TelemetryExporterType.AzureMonitor:
                openTelemetry.UseAzureMonitor();
                break;
            case TelemetryExporterType.Otlp:
                openTelemetry
                    .WithTracing(tracing => tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation())
                    .WithMetrics(metrics => metrics
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation())
                    .UseOtlpExporter();
                break;
            case TelemetryExporterType.None:
                break;
        }

        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });
    }

    private static TelemetryExporterType ResolveTelemetryExporter(IConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        {
            return TelemetryExporterType.AzureMonitor;
        }

        return !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"])
            ? TelemetryExporterType.Otlp
            : TelemetryExporterType.None;
    }

    public static void MapDefaultEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live")
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = _ => true
        });
    }
}