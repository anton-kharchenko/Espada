using Asp.Versioning;
using AutoMapper;
using Espada.Api.Filters;
using Espada.Api.Mappings;
using Espada.Api.Middlewares;
using Espada.Api.OpenApi;
using Espada.Comms.Core.Security;
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
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer<ApiKeySecuritySchemeTransformer>();
            options.AddOperationTransformer<ApiKeySecurityRequirementTransformer>();
        });

        services.AddEspadaApiKeyAuthentication(configuration);

        services.AddAuthorization();
        services.AddHealthChecks();
        services.AddAutoMapper(_ => { }, typeof(ApiMappingProfile));

        return services;
    }
}