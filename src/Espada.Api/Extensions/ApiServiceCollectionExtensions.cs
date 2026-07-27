using Asp.Versioning;
using Espada.Api.Filters;
using Espada.Api.Mappings;
using Espada.Api.Middlewares;
using Espada.Api.OpenApi;
using Espada.Comms.Core.Security;
using Microsoft.AspNetCore.Mvc;
using Espada.Api.Authentication;
using Espada.Api.Authentication.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
        services.AddScoped<WorkspaceMembershipAuthorizationFilter>();
        services.AddControllers(options =>
        {
            options.Filters.AddService<WorkspaceMembershipAuthorizationFilter>();
            options.Filters.AddService<ValidationFilter>();
        });
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

        EntraExternalIdOptions entra = configuration
            .GetSection(EntraExternalIdConstants.SectionName)
            .Get<EntraExternalIdOptions>() ?? new EntraExternalIdOptions();
        AuthenticationBuilder authentication = services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Espada";
                options.DefaultChallengeScheme = "Espada";
            })
            .AddPolicyScheme("Espada", "Espada authentication", options =>
            {
                options.ForwardDefaultSelector = context =>
                    entra.IsConfigured()
                    && context.Request.Headers.Authorization.ToString()
                        .StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? JwtBearerDefaults.AuthenticationScheme
                        : ApiKeyAuthenticationDefaults.AuthenticationScheme;
            });
        if (entra.IsConfigured())
        {
            authentication.AddJwtBearer(options =>
            {
                options.Authority = entra.Authority;
                options.Audience = entra.Audience;
                options.RequireHttpsMetadata = true;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                };
            });
        }

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireAssertion(context =>
                    context.User.Identity?.AuthenticationType
                        == ApiKeyAuthenticationDefaults.AuthenticationScheme
                    || context.User.FindAll("scp")
                        .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                        .Contains(entra.Scope, StringComparer.Ordinal))
                .Build();
        });
        services.AddHealthChecks();
        services.AddAutoMapper(_ => { }, typeof(ApiMappingProfile));

        return services;
    }
}