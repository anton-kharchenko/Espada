using Espada.Application.Constants;
using Espada.Infrastructure.Database;
using Espada.Mcp.Constants;
using Espada.Mcp.Services;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.RateLimiting;

namespace Espada.Mcp.Security
{
    internal static class McpAuthorizationServiceCollectionExtensions
    {
        public static IServiceCollection AddMcpAuthorization(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            McpAuthorizationOptions authorizationOptions = configuration
                                                               .GetSection(McpAuthorizationConstants.SectionName)
                                                               .Get<McpAuthorizationOptions>()
                                                           ?? new McpAuthorizationOptions();
            if (!authorizationOptions.IsValid())
            {
                throw new InvalidOperationException(
                    "MCP authorization configuration is invalid.");
            }

            services
                .AddOptions<McpAuthorizationOptions>()
                .Bind(
                    configuration.GetSection(
                        McpAuthorizationConstants.SectionName))
                .Validate(
                    options => options.IsValid(),
                    "MCP authorization configuration is invalid.")
                .ValidateOnStart();
            services.AddHttpContextAccessor();
            services.AddScoped<DynamicClientRegistrationService>();
            services.AddScoped<McpAuthorizationGrantService>();
            services.AddAntiforgery(options =>
            {
                options.FormFieldName =
                    McpAuthorizationConstants.AntiforgeryFieldName;
                options.Cookie.Name =
                    authorizationOptions.Issuer.Scheme.Equals(
                        Uri.UriSchemeHttps,
                        StringComparison.OrdinalIgnoreCase)
                        ? "__Host-Espada.Mcp.Antiforgery"
                        : "Espada.Mcp.Antiforgery";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy =
                    authorizationOptions.Issuer.Scheme.Equals(
                        Uri.UriSchemeHttps,
                        StringComparison.OrdinalIgnoreCase)
                        ? CookieSecurePolicy.Always
                        : CookieSecurePolicy.SameAsRequest;
            });
            AuthenticationBuilder authentication = services
                .AddAuthentication();
            authentication.AddCookie(
                McpAuthorizationConstants.AuthorityCookieScheme,
                options =>
                {
                    options.Cookie.Name =
                        authorizationOptions.Issuer.Scheme.Equals(
                            Uri.UriSchemeHttps,
                            StringComparison.OrdinalIgnoreCase)
                            ? "__Host-Espada.Mcp.Authority"
                            : "Espada.Mcp.Authority";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.Cookie.SecurePolicy =
                        authorizationOptions.Issuer.Scheme.Equals(
                            Uri.UriSchemeHttps,
                            StringComparison.OrdinalIgnoreCase)
                            ? CookieSecurePolicy.Always
                            : CookieSecurePolicy.SameAsRequest;
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = false;
                });
            if (authorizationOptions.HasEntraAuthority)
            {
                authentication.AddOpenIdConnect(
                    McpAuthorizationConstants.EntraScheme,
                    options =>
                    {
                        options.Authority =
                            authorizationOptions.EntraAuthority;
                        options.ClientId =
                            authorizationOptions.EntraClientId;
                        options.ClientSecret =
                            authorizationOptions.EntraClientSecret;
                        options.SignInScheme =
                            McpAuthorizationConstants.AuthorityCookieScheme;
                        options.ResponseType =
                            OpenIddictConstants.ResponseTypes.Code;
                        options.UsePkce = true;
                        options.SaveTokens = false;
                        options.GetClaimsFromUserInfoEndpoint = false;
                        options.Events.OnTokenValidated = context =>
                        {
                            ClaimsIdentity? identity =
                                context.Principal?.Identity as ClaimsIdentity;
                            string? issuer =
                                context.Principal?.FindFirst("iss")?.Value;
                            string? subject =
                                context.Principal?.FindFirst(
                                    OpenIddictConstants.Claims.Subject)?.Value;
                            if (identity is not null
                                && !string.IsNullOrWhiteSpace(issuer)
                                && !string.IsNullOrWhiteSpace(subject))
                            {
                                identity.AddClaim(
                                    new Claim(
                                        McpAuthorizationConstants
                                            .IdentityIssuerClaim,
                                        issuer));
                            }

                            return Task.CompletedTask;
                        };
                    });
            }

            services.AddAuthorization(options =>
                options.AddPolicy(
                    McpAuthorizationConstants.AccessPolicy,
                    policy =>
                    {
                        policy.AddAuthenticationSchemes(
                            OpenIddictValidationAspNetCoreDefaults
                                .AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                    }));
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode =
                    StatusCodes.Status429TooManyRequests;
                options.AddPolicy(
                    McpAuthorizationConstants.RateLimitPolicy,
                    context => CreateRateLimitPartition(
                        context,
                        authorizationOptions));
                options.AddPolicy(
                    McpAuthorizationConstants.RegistrationRateLimitPolicy,
                    context => RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                            QueueProcessingOrder =
                                QueueProcessingOrder.OldestFirst,
                            AutoReplenishment = true
                        }));
            });
            services
                .AddOpenIddict()
                .AddCore(options =>
                    options
                        .UseEntityFrameworkCore()
                        .UseDbContext<EspadaDbContext>()
                        .ReplaceDefaultEntities<Guid>())
                .AddServer(options =>
                {
                    options.SetIssuer(authorizationOptions.Issuer);
                    options.SetAuthorizationEndpointUris(
                        McpAuthorizationConstants.AuthorizationEndpoint);
                    options.SetTokenEndpointUris(
                        McpAuthorizationConstants.TokenEndpoint);
                    options.SetRevocationEndpointUris(
                        McpAuthorizationConstants.RevocationEndpoint);
                    options.AllowAuthorizationCodeFlow();
                    options.AllowRefreshTokenFlow();
                    options.RequireProofKeyForCodeExchange();
                    options.Configure(serverOptions =>
                    {
                        serverOptions.CodeChallengeMethods.Clear();
                        serverOptions.CodeChallengeMethods.Add(
                            OpenIddictConstants.CodeChallengeMethods.Sha256);
                    });
                    options.RegisterResources(
                        authorizationOptions.Resource.AbsoluteUri);
                    options.RegisterScopes(
                        ApplicationScopeConstants.All
                            .Append(
                                McpAuthorizationConstants
                                    .OfflineAccessScope)
                            .ToArray());
                    options.SetAuthorizationCodeLifetime(
                        TimeSpan.FromMinutes(
                            McpAuthorizationConstants
                                .AuthorizationCodeLifetimeMinutes));
                    options.SetAccessTokenLifetime(
                        TimeSpan.FromMinutes(
                            McpAuthorizationConstants
                                .AccessTokenLifetimeMinutes));
                    options.SetRefreshTokenLifetime(
                        TimeSpan.FromDays(
                            McpAuthorizationConstants
                                .RefreshTokenLifetimeDays));
                    options.SetRefreshTokenReuseLeeway(TimeSpan.Zero);
                    options.DisableSlidingRefreshTokenExpiration();
                    options.UseReferenceAccessTokens();
                    options.UseReferenceRefreshTokens();

                    if (authorizationOptions.Issuer.Scheme.Equals(
                            Uri.UriSchemeHttp,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        options.AddDevelopmentEncryptionCertificate();
                        options.AddDevelopmentSigningCertificate();
                        options.UseAspNetCore()
                            .DisableTransportSecurityRequirement()
                            .EnableAuthorizationEndpointPassthrough()
                            .EnableStatusCodePagesIntegration();
                    }
                    else
                    {
                        AddTokenProtectionCertificates(
                            options,
                            authorizationOptions);
                        options.UseAspNetCore()
                            .EnableAuthorizationEndpointPassthrough()
                            .EnableStatusCodePagesIntegration();
                    }
                })
                .AddValidation(options =>
                {
                    options.AddAudiences(
                        authorizationOptions.Resource.AbsoluteUri);
                    options.UseLocalServer();
                    options.EnableAuthorizationEntryValidation();
                    options.EnableTokenEntryValidation();
                    options.UseAspNetCore();
                });

            return services;
        }

        private static void AddTokenProtectionCertificates(
            OpenIddictServerBuilder builder,
            McpAuthorizationOptions options)
        {
            if (!string.IsNullOrWhiteSpace(
                    options.EncryptionCertificateBase64)
                && !string.IsNullOrWhiteSpace(
                    options.SigningCertificateBase64))
            {
                builder.AddEncryptionCertificate(
                    LoadCertificate(
                        options.EncryptionCertificateBase64,
                        "encryption"));
                builder.AddSigningCertificate(
                    LoadCertificate(
                        options.SigningCertificateBase64,
                        "signing"));
                return;
            }

            builder.AddEncryptionCertificate(
                options.EncryptionCertificateThumbprint!);
            builder.AddSigningCertificate(
                options.SigningCertificateThumbprint!);
        }

        private static X509Certificate2 LoadCertificate(
            string value,
            string purpose)
        {
            try
            {
                byte[] certificate = Convert.FromBase64String(value);
                X509Certificate2 result = X509CertificateLoader.LoadPkcs12(
                    certificate,
                    null,
                    X509KeyStorageFlags.EphemeralKeySet);
                return result.HasPrivateKey
                    ? result
                    : throw new InvalidOperationException(
                        $"The MCP {purpose} certificate does not contain a private key.");
            }
            catch (FormatException exception)
            {
                throw new InvalidOperationException(
                    $"The MCP {purpose} certificate is not valid base64.",
                    exception);
            }
            catch (CryptographicException exception)
            {
                throw new InvalidOperationException(
                    $"The MCP {purpose} certificate is not a valid passwordless PKCS#12 payload.",
                    exception);
            }
        }

        private static RateLimitPartition<string> CreateRateLimitPartition(
            HttpContext context,
            McpAuthorizationOptions options)
        {
            string workspaceId = context.User.FindFirstValue(
                                     McpAuthorizationConstants.WorkspaceIdClaim)
                                 ?? "bootstrap";
            string clientId = context.User.FindFirstValue(
                                  McpAuthorizationConstants.ClientIdentityClaim)
                              ?? "anonymous";
            int tokenCeiling = int.TryParse(
                context.User.FindFirstValue(
                    McpAuthorizationConstants.RateCeilingClaim),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int parsedCeiling)
                ? parsedCeiling
                : options.DefaultRateCeilingPerMinute;
            int effectiveCeiling = Math.Max(
                1,
                Math.Min(
                    tokenCeiling,
                    options.MaximumRateCeilingPerMinute));
            int burst = Math.Min(options.Burst, effectiveCeiling);

            return RateLimitPartition.GetTokenBucketLimiter(
                $"{workspaceId}:{clientId}",
                _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = burst,
                    TokensPerPeriod = 1,
                    ReplenishmentPeriod = TimeSpan.FromMinutes(
                        1d / effectiveCeiling),
                    AutoReplenishment = true,
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
        }
    }
}