using Asp.Versioning;
using Espada.Api.Filters;
using Espada.Api.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace Espada.Api.Extensions;

internal static class ApiServiceCollectionExtensions
{
    public static IServiceCollection ConfigureApi(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        services.AddProblemDetails();
        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddScoped<ValidationFilter>();

        services.AddControllers(options => options.Filters.AddService<ValidationFilter>());

        services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc();

        services.AddAuthorization();
        services.AddHealthChecks();

        return services;
    }
}