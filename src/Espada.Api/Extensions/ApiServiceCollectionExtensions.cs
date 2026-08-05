using Asp.Versioning;
using Espada.Api.Authentication;
using Espada.Api.Authentication.Constants;
using Espada.Api.Filters;
using Espada.Api.Mappings;
using Espada.Api.Middlewares;
using Espada.Api.OpenApi;
using Espada.Application.Contracts.Security;
using Espada.Comms.Core.Constants;
using Espada.Comms.Core.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Security.Claims;

namespace Espada.Api.Extensions
{
    internal static class ApiServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureApi(this IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(environment);

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor
                    | ForwardedHeaders.XForwardedHost
                    | ForwardedHeaders.XForwardedProto;
                if (!options.KnownProxies.Contains(IPAddress.Loopback))
                {
                    options.KnownProxies.Add(IPAddress.Loopback);
                }
            });
            services.AddProblemDetails();
            services.AddExceptionHandler<ApiExceptionHandler>();
            services.AddScoped<ValidationFilter>();
            services.AddScoped<WorkspaceMembershipAuthorizationFilter>();
            services.AddScoped<WebConsoleRequestFilter>();
            services.AddScoped<WebConsoleWorkspaceFilter>();
            services.AddScoped<WebConsoleOwnerFilter>();
            services.AddHttpContextAccessor();
            services.AddScoped<
                IRequestPrincipalAccessor,
                ApiRequestPrincipalAccessor>();
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
            WebConsoleOptions webConsole = configuration
                .GetSection(WebConsoleOptions.SectionName)
                .Get<WebConsoleOptions>() ?? new WebConsoleOptions();
            if (!webConsole.IsValid()
                || webConsole.Mode == WebConsoleMode.Cloud
                && !entra.IsInteractiveLoginConfigured())
            {
                throw new InvalidOperationException(
                    "Web Console authentication configuration is invalid.");
            }

            services
                .AddOptions<WebConsoleOptions>()
                .Bind(configuration.GetSection(WebConsoleOptions.SectionName))
                .Validate(
                    options => options.IsValid(),
                    "Web Console authentication configuration is invalid.")
                .ValidateOnStart();
            services.AddAntiforgery(options =>
            {
                options.HeaderName =
                    WebConsoleAuthenticationConstants.AntiforgeryHeaderName;
                options.Cookie.Name =
                    webConsole.Mode == WebConsoleMode.Cloud
                        ? "__Host-"
                          + WebConsoleAuthenticationConstants
                              .AntiforgeryCookieName
                        : WebConsoleAuthenticationConstants
                            .AntiforgeryCookieName;
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Path = "/";
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy =
                    webConsole.Mode == WebConsoleMode.Cloud
                        ? CookieSecurePolicy.Always
                        : CookieSecurePolicy.SameAsRequest;
            });
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
                            : ApiKeyAuthenticationConstants.AuthenticationScheme;
                });
            authentication.AddCookie(
                WebConsoleAuthenticationConstants.CookieScheme,
                options =>
                {
                    options.Cookie.Name =
                        webConsole.Mode == WebConsoleMode.Cloud
                            ? "__Host-"
                              + WebConsoleAuthenticationConstants.CookieName
                            : WebConsoleAuthenticationConstants.CookieName;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                    options.Cookie.Path = "/";
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.Cookie.SecurePolicy =
                        webConsole.Mode == WebConsoleMode.Cloud
                            ? CookieSecurePolicy.Always
                            : CookieSecurePolicy.SameAsRequest;
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = false;
                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode =
                            StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    };
                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode =
                            StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    };
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

            if (webConsole.Mode == WebConsoleMode.Cloud)
            {
                authentication.AddOpenIdConnect(
                    WebConsoleAuthenticationConstants
                        .CloudOpenIdConnectScheme,
                    options =>
                    {
                        options.Authority = entra.Authority;
                        options.ClientId = entra.ClientId;
                        options.ClientSecret = entra.ClientSecret;
                        options.SignInScheme =
                            WebConsoleAuthenticationConstants.CookieScheme;
                        options.ResponseType = "code";
                        options.UsePkce = true;
                        options.SaveTokens = false;
                        options.MapInboundClaims = false;
                        options.GetClaimsFromUserInfoEndpoint = false;
                        options.CallbackPath = "/bff/auth/signin-oidc";
                        options.Scope.Clear();
                        options.Scope.Add("openid");
                        options.Scope.Add("profile");
                        options.Scope.Add("email");
                        options.Events.OnTokenValidated = context =>
                        {
                            string? issuer = context.Principal?.FindFirst(
                                WebConsoleAuthenticationConstants
                                    .IdentityIssuerClaim)?.Value;
                            string? subject = context.Principal?.FindFirst(
                                WebConsoleAuthenticationConstants
                                    .IdentitySubjectClaim)?.Value;
                            if (string.IsNullOrWhiteSpace(issuer)
                                || string.IsNullOrWhiteSpace(subject))
                            {
                                context.Fail(
                                    "The Entra identity does not contain iss and sub claims.");
                            }
                            else if (context.Principal?.Identity
                                     is ClaimsIdentity identity)
                            {
                                identity.AddClaim(
                                    new Claim(
                                        WebConsoleAuthenticationConstants
                                            .SessionIdentityClaim,
                                        Guid.NewGuid().ToString("D")));
                            }

                            return Task.CompletedTask;
                        };
                    });
            }

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context =>
                        context.User.Identity?.AuthenticationType
                        == ApiKeyAuthenticationConstants.AuthenticationScheme
                        || context.User.FindAll("scp")
                            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                            .Contains(entra.Scope, StringComparer.Ordinal))
                    .Build();
                options.AddPolicy(
                    WebConsoleAuthenticationConstants.AccessPolicy,
                    policy =>
                    {
                        policy.AddAuthenticationSchemes(
                            WebConsoleAuthenticationConstants.CookieScheme);
                        policy.RequireAuthenticatedUser();
                    });
            });
            services.AddHealthChecks();
            services.AddAutoMapper(_ => { }, typeof(ApiMappingProfile));

            return services;
        }
    }
}