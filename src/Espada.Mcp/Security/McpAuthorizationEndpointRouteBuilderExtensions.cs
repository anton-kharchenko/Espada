using Espada.Application.Contracts.Time;
using Espada.Infrastructure.Security;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using Espada.Application.Constants;
using Espada.Mcp.Constants;
using Espada.Mcp.Services;
using Espada.Mcp.Requests;
using Espada.Mcp.Responses;
using Espada.Mcp.Models;
using Espada.Infrastructure.Constants;

namespace Espada.Mcp.Security
{
    internal static class McpAuthorizationEndpointRouteBuilderExtensions
    {
        private static readonly TimeSpan BootstrapLifetime =
            TimeSpan.FromMinutes(5);

        public static IEndpointRouteBuilder MapMcpAuthorizationEndpoints(
            this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost(
                    McpAuthorizationConstants.RegistrationEndpoint,
                    RegisterClientAsync)
                .RequireRateLimiting(
                    McpAuthorizationConstants.RegistrationRateLimitPolicy);
            endpoints.MapPost(
                McpAuthorizationConstants.BootstrapLinkEndpoint,
                CreateBootstrapLinkAsync);
            endpoints.MapGet(
                McpAuthorizationConstants.BootstrapEndpoint,
                ShowBootstrapPage);
            endpoints.MapPost(
                McpAuthorizationConstants.BootstrapEndpoint,
                ConsumeBootstrapCodeAsync);
            endpoints.MapGet(
                "/.well-known/oauth-protected-resource",
                GetProtectedResourceMetadata);
            endpoints.MapGet(
                "/.well-known/oauth-protected-resource/mcp",
                GetProtectedResourceMetadata);
            endpoints.MapGet(
                McpAuthorizationConstants.AuthorizationEndpoint,
                ShowAuthorizationConsentAsync);
            endpoints.MapPost(
                McpAuthorizationConstants.AuthorizationEndpoint,
                AcceptAuthorizationConsentAsync);

            return endpoints;
        }

        private static async Task<IResult> RegisterClientAsync(
            DynamicClientRegistrationRequest request,
            DynamicClientRegistrationService service,
            CancellationToken cancellationToken)
        {
            try
            {
                DynamicClientRegistrationResponse response =
                    await service.RegisterAsync(
                        request,
                        cancellationToken);
                return Results.Json(
                    response,
                    statusCode: StatusCodes.Status201Created);
            }
            catch (InvalidClientMetadataException exception)
            {
                return Results.BadRequest(
                    new OAuthErrorResponse(
                        "invalid_client_metadata",
                        exception.Message));
            }
        }

        private static async Task<IResult> CreateBootstrapLinkAsync(
            HttpContext context,
            [FromQuery] string? returnUrl,
            OneTimeBootstrapCodeStore store,
            IOptions<McpAuthorizationOptions> options,
            CancellationToken cancellationToken)
        {
            if (!IsLoopback(context.Connection.RemoteIpAddress))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            string validatedReturnUrl = ValidateLocalReturnUrl(returnUrl);
            string code = await store.CreateAsync(
                BootstrapCodePurposeConstants.McpAuthority,
                options.Value.LocalIdentityIssuer,
                options.Value.LocalIdentitySubject,
                BootstrapLifetime,
                cancellationToken);
            Uri bootstrapEndpoint = new(
                options.Value.Issuer,
                McpAuthorizationConstants.BootstrapEndpoint);
            string fragment = QueryHelpers.AddQueryString(
                string.Empty,
                new Dictionary<string, string?> { ["code"] = code, ["returnUrl"] = validatedReturnUrl }).TrimStart('?');

            return Results.Ok(
                new BootstrapLinkResponse(
                    $"{bootstrapEndpoint.AbsoluteUri}#{fragment}",
                    (int)BootstrapLifetime.TotalSeconds));
        }

        private static IResult ShowBootstrapPage(HttpContext context)
        {
            string nonce = WebEncoders.Base64UrlEncode(
                RandomNumberGenerator.GetBytes(16));
            context.Response.Headers.ContentSecurityPolicy =
                "default-src 'none'; "
                + $"script-src 'nonce-{nonce}'; "
                + "style-src 'unsafe-inline'; "
                + "form-action 'self'; base-uri 'none'; frame-ancestors 'none'";
            context.Response.Headers["Referrer-Policy"] = "no-referrer";
            return Results.Content(
                BootstrapPageRenderer.Render(nonce),
                "text/html; charset=utf-8");
        }

        private static async Task<IResult> ConsumeBootstrapCodeAsync(
            HttpContext context,
            OneTimeBootstrapCodeStore store,
            IClockService clockService,
            CancellationToken cancellationToken)
        {
            IFormCollection form = await context.Request.ReadFormAsync(
                cancellationToken);
            string code = form["code"].ToString();
            string? returnUrl = form["returnUrl"].ToString();
            BootstrapIdentity? identity = await store.ConsumeAsync(
                BootstrapCodePurposeConstants.McpAuthority,
                code,
                cancellationToken);
            if (identity is null)
            {
                return Results.BadRequest(
                    new OAuthErrorResponse(
                        "invalid_bootstrap_code",
                        "The bootstrap link is invalid, expired, or already used."));
            }

            ClaimsIdentity claimsIdentity = new(
                McpAuthorizationConstants.AuthorityCookieScheme);
            claimsIdentity.AddClaim(
                new Claim(
                    OpenIddictConstants.Claims.Subject,
                    identity.IdentitySubject));
            claimsIdentity.AddClaim(
                new Claim(
                    McpAuthorizationConstants.IdentityIssuerClaim,
                    identity.IdentityIssuer));
            ClaimsPrincipal principal = new(claimsIdentity);
            await context.SignInAsync(
                McpAuthorizationConstants.AuthorityCookieScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false, AllowRefresh = false, ExpiresUtc = clockService.UtcNow.AddHours(8)
                });

            string validatedReturnUrl = ValidateLocalReturnUrl(returnUrl);
            return string.IsNullOrEmpty(validatedReturnUrl)
                ? Results.Ok()
                : Results.LocalRedirect(validatedReturnUrl);
        }

        private static IResult GetProtectedResourceMetadata(
            IOptions<McpAuthorizationOptions> options)
        {
            return Results.Ok(
                new ProtectedResourceMetadataResponse(
                    options.Value.Resource.AbsoluteUri,
                    McpAuthorizationConstants.ResourceName,
                    [options.Value.Issuer.AbsoluteUri],
                    ApplicationScopeConstants.All
                        .Append(
                            McpAuthorizationConstants.OfflineAccessScope)
                        .Order()
                        .ToArray(),
                    ["header"]));
        }

        private static async Task<IResult> ShowAuthorizationConsentAsync(
            HttpContext context,
            McpAuthorizationGrantService grantService,
            IAntiforgery antiforgery,
            IOptions<McpAuthorizationOptions> options,
            CancellationToken cancellationToken)
        {
            OpenIddictRequest request =
                context.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException(
                    "The OpenIddict authorization request is unavailable.");
            ClaimsPrincipal? localIdentity =
                await AuthenticateLocalIdentityAsync(context);
            if (localIdentity is null)
            {
                if (options.Value.HasEntraAuthority)
                {
                    return Results.Challenge(
                        new AuthenticationProperties
                        {
                            RedirectUri =
                                context.Request.PathBase
                                + context.Request.Path
                                + context.Request.QueryString
                        },
                        [McpAuthorizationConstants.EntraScheme]);
                }

                SetHtmlSecurityHeaders(context);
                return Results.Content(
                    AuthorizationConsentPageRenderer.RenderSessionRequired(),
                    "text/html; charset=utf-8",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            try
            {
                McpAuthorizationGrant grant =
                    await grantService.CreateAsync(
                        request,
                        localIdentity,
                        cancellationToken);
                AntiforgeryTokenSet tokens =
                    antiforgery.GetAndStoreTokens(context);
                string requestToken = tokens.RequestToken
                                      ?? throw new InvalidOperationException(
                                          "The antiforgery request token is unavailable.");
                SetHtmlSecurityHeaders(context);
                return Results.Content(
                    AuthorizationConsentPageRenderer.Render(
                        grant,
                        request,
                        requestToken),
                    "text/html; charset=utf-8");
            }
            catch (InvalidAuthorizationRequestException exception)
            {
                return CreateAuthorizationForbid(
                    exception.Error,
                    exception.Message);
            }
        }

        private static async Task<IResult> AcceptAuthorizationConsentAsync(
            HttpContext context,
            McpAuthorizationGrantService grantService,
            IAntiforgery antiforgery,
            CancellationToken cancellationToken)
        {
            OpenIddictRequest request =
                context.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException(
                    "The OpenIddict authorization request is unavailable.");
            ClaimsPrincipal? localIdentity =
                await AuthenticateLocalIdentityAsync(context);
            if (localIdentity is null)
            {
                return CreateAuthorizationForbid(
                    OpenIddictConstants.Errors.AccessDenied,
                    "The local authority session is missing or expired.");
            }

            await antiforgery.ValidateRequestAsync(context);
            IFormCollection form = await context.Request.ReadFormAsync(
                cancellationToken);
            if (form["decision"].ToString()
                .Equals("deny", StringComparison.Ordinal))
            {
                return CreateAuthorizationForbid(
                    OpenIddictConstants.Errors.AccessDenied,
                    "The resource owner denied the authorization request.");
            }

            if (!form["decision"].ToString()
                    .Equals("allow", StringComparison.Ordinal))
            {
                return CreateAuthorizationForbid(
                    OpenIddictConstants.Errors.InvalidRequest,
                    "The authorization decision is invalid.");
            }

            try
            {
                McpAuthorizationGrant grant =
                    await grantService.CreateAsync(
                        request,
                        localIdentity,
                        cancellationToken);
                ClaimsPrincipal principal = CreateTokenPrincipal(grant);
                return Results.SignIn(
                    principal,
                    authenticationScheme:
                    OpenIddictServerAspNetCoreDefaults
                        .AuthenticationScheme);
            }
            catch (InvalidAuthorizationRequestException exception)
            {
                return CreateAuthorizationForbid(
                    exception.Error,
                    exception.Message);
            }
        }

        private static async Task<ClaimsPrincipal?>
            AuthenticateLocalIdentityAsync(HttpContext context)
        {
            AuthenticateResult authentication =
                await context.AuthenticateAsync(
                    McpAuthorizationConstants.AuthorityCookieScheme);
            return authentication.Succeeded
                ? authentication.Principal
                : null;
        }

        private static ClaimsPrincipal CreateTokenPrincipal(
            McpAuthorizationGrant grant)
        {
            ClaimsIdentity identity = new(
                TokenValidationParameters.DefaultAuthenticationType,
                OpenIddictConstants.Claims.Name,
                OpenIddictConstants.Claims.Role);
            identity.AddClaim(
                new Claim(
                    OpenIddictConstants.Claims.Subject,
                    grant.IdentitySubject));
            identity.AddClaim(
                new Claim(
                    McpAuthorizationConstants.IdentityIssuerClaim,
                    grant.IdentityIssuer));
            identity.AddClaim(
                new Claim(
                    McpAuthorizationConstants.ClientIdentityClaim,
                    grant.ClientId));
            identity.AddClaim(
                new Claim(
                    McpAuthorizationConstants.RateCeilingClaim,
                    grant.RateCeilingPerMinute.ToString(
                        CultureInfo.InvariantCulture)));
            if (grant.WorkspaceId.HasValue)
            {
                identity.AddClaim(
                    new Claim(
                        McpAuthorizationConstants.WorkspaceIdClaim,
                        grant.WorkspaceId.Value.ToString("D")));
            }

            ClaimsPrincipal principal = new(identity);
            principal.SetScopes(grant.Scopes);
            principal.SetResources(grant.Resource);
            foreach (Claim claim in principal.Claims)
            {
                claim.SetDestinations(
                    OpenIddictConstants.Destinations.AccessToken);
            }

            return principal;
        }

        private static IResult CreateAuthorizationForbid(
            string error,
            string description)
        {
            return Results.Forbid(
                new AuthenticationProperties(
                    new Dictionary<string, string?>
                    {
                        [
                            OpenIddictServerAspNetCoreConstants.Properties.Error
                        ] = error,
                        [
                            OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription
                        ] = description
                    }),
                [
                    OpenIddictServerAspNetCoreDefaults
                        .AuthenticationScheme
                ]);
        }

        private static void SetHtmlSecurityHeaders(HttpContext context)
        {
            context.Response.Headers.ContentSecurityPolicy =
                "default-src 'none'; style-src 'unsafe-inline'; "
                + "form-action 'self'; base-uri 'none'; frame-ancestors 'none'";
            context.Response.Headers["Referrer-Policy"] = "no-referrer";
        }

        private static bool IsLoopback(IPAddress? address)
        {
            return address is not null && IPAddress.IsLoopback(address);
        }

        private static string ValidateLocalReturnUrl(string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return string.Empty;
            }

            return returnUrl.StartsWith("/", StringComparison.Ordinal)
                   && !returnUrl.StartsWith("//", StringComparison.Ordinal)
                   && !returnUrl.StartsWith("/\\", StringComparison.Ordinal)
                ? returnUrl
                : throw new BadHttpRequestException(
                    "returnUrl must be a local absolute path.");
        }
    }
}