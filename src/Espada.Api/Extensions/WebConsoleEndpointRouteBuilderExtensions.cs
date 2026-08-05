using AutoMapper;
using Espada.Api.Authentication;
using Espada.Api.Authentication.Constants;
using Espada.Api.Contracts.Responses.WebConsole;
using Espada.Application.Contracts.Time;
using Espada.Application.UseCases.Workspaces.Queries.ListAccessibleWorkspaces;
using Espada.Domain.Rules;
using Espada.Infrastructure.Constants;
using Espada.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Espada.Api.Extensions
{
    internal static class WebConsoleEndpointRouteBuilderExtensions
    {
        private static readonly TimeSpan BootstrapLifetime =
            TimeSpan.FromMinutes(5);

        private static readonly TimeSpan SessionLifetime =
            TimeSpan.FromHours(8);

        private static readonly TimeSpan AntiforgeryLifetime =
            TimeSpan.FromHours(1);

        public static IEndpointRouteBuilder MapWebConsoleEndpoints(
            this IEndpointRouteBuilder endpoints)
        {
            RouteGroupBuilder authentication = endpoints
                .MapGroup("/bff/auth")
                .WithTags("Web Console Authentication")
                .AllowAnonymous();
            authentication.MapPost(
                "/bootstrap-link",
                CreateBootstrapLinkAsync);
            authentication.MapGet(
                "/bootstrap",
                ShowBootstrapPage);
            authentication.MapPost(
                "/bootstrap",
                ConsumeBootstrapCodeAsync);
            authentication.MapGet(
                "/login",
                BeginCloudLogin);

            RouteGroupBuilder bff = endpoints
                .MapGroup("/bff")
                .WithTags("Web Console");
            bff.MapGet("/session", GetSessionAsync)
                .AllowAnonymous();

            RouteGroupBuilder protectedBff = bff
                .MapGroup(string.Empty)
                .RequireAuthorization(
                    WebConsoleAuthenticationConstants.AccessPolicy)
                .AddEndpointFilter<WebConsoleRequestFilter>();
            protectedBff.MapPost(
                "/session/logout",
                LogoutAsync);
            protectedBff.MapLocalSetupEndpoints();
            protectedBff.MapWebConsoleWorkspaceEndpoints();

            return endpoints;
        }

        private static async Task<IResult> CreateBootstrapLinkAsync(
            HttpContext context,
            [FromQuery] string? returnUrl,
            OneTimeBootstrapCodeStore store,
            IOptions<WebConsoleOptions> options,
            CancellationToken cancellationToken)
        {
            if (options.Value.Mode != WebConsoleMode.Local)
            {
                return Results.NotFound();
            }

            if (!WebConsoleRequestSecurity.IsAllowed(
                    context,
                    options.Value))
            {
                return Results.StatusCode(
                    StatusCodes.Status403Forbidden);
            }

            string validatedReturnUrl =
                WebConsoleRequestSecurity.ValidateLocalReturnUrl(returnUrl);
            string code = await store.CreateAsync(
                BootstrapCodePurposeConstants.ConsoleSession,
                options.Value.LocalIdentityIssuer,
                options.Value.LocalIdentitySubject,
                BootstrapLifetime,
                cancellationToken);
            string bootstrapEndpoint =
                context.Request.PathBase.Add(
                    "/bff/auth/bootstrap");
            string fragment = QueryHelpers.AddQueryString(
                    string.Empty,
                    new Dictionary<string, string?>
                    {
                        ["code"] = code,
                        ["returnUrl"] = validatedReturnUrl
                    })
                .TrimStart('?');

            return Results.Ok(
                new ConsoleBootstrapLinkResponse(
                    $"{bootstrapEndpoint}#{fragment}",
                    (int)BootstrapLifetime.TotalSeconds));
        }

        private static IResult ShowBootstrapPage(
            HttpContext context,
            IOptions<WebConsoleOptions> options)
        {
            if (options.Value.Mode != WebConsoleMode.Local)
            {
                return Results.NotFound();
            }

            if (!WebConsoleRequestSecurity.IsAllowed(
                    context,
                    options.Value))
            {
                return Results.StatusCode(
                    StatusCodes.Status403Forbidden);
            }

            string nonce = WebEncoders.Base64UrlEncode(
                RandomNumberGenerator.GetBytes(16));
            context.Response.Headers.ContentSecurityPolicy =
                "default-src 'none'; "
                + $"script-src 'nonce-{nonce}'; "
                + "style-src 'unsafe-inline'; "
                + "connect-src 'self'; "
                + "form-action 'self'; "
                + "base-uri 'none'; "
                + "frame-ancestors 'none'";
            context.Response.Headers["Referrer-Policy"] = "no-referrer";

            return Results.Content(
                WebConsoleBootstrapPageRenderer.Render(nonce),
                "text/html; charset=utf-8");
        }

        private static async Task<IResult> ConsumeBootstrapCodeAsync(
            HttpContext context,
            OneTimeBootstrapCodeStore store,
            IClockService clockService,
            IOptions<WebConsoleOptions> options,
            CancellationToken cancellationToken)
        {
            if (options.Value.Mode != WebConsoleMode.Local)
            {
                return Results.NotFound();
            }

            if (!WebConsoleRequestSecurity.IsAllowed(
                    context,
                    options.Value))
            {
                return Results.StatusCode(
                    StatusCodes.Status403Forbidden);
            }

            if (!context.Request.HasFormContentType)
            {
                return Results.BadRequest(
                    new
                    {
                        code = "invalid_argument",
                        message = "A form body is required."
                    });
            }

            IFormCollection form = await context.Request.ReadFormAsync(
                cancellationToken);
            BootstrapIdentity? identity = await store.ConsumeAsync(
                BootstrapCodePurposeConstants.ConsoleSession,
                form["code"].ToString(),
                cancellationToken);
            if (identity is null)
            {
                return Results.BadRequest(
                    new
                    {
                        code = "invalid_bootstrap_code",
                        message =
                            "The bootstrap link is invalid, expired, or already used."
                    });
            }

            ClaimsIdentity claimsIdentity = new(
                WebConsoleAuthenticationConstants.CookieScheme);
            claimsIdentity.AddClaim(
                new Claim(
                    WebConsoleAuthenticationConstants.IdentityIssuerClaim,
                    identity.IdentityIssuer));
            claimsIdentity.AddClaim(
                new Claim(
                    WebConsoleAuthenticationConstants.IdentitySubjectClaim,
                    identity.IdentitySubject));
            claimsIdentity.AddClaim(
                new Claim(
                    ClaimTypes.Name,
                    identity.IdentitySubject));
            claimsIdentity.AddClaim(
                new Claim(
                    WebConsoleAuthenticationConstants
                        .SessionIdentityClaim,
                    Guid.NewGuid().ToString("D")));
            await context.SignInAsync(
                WebConsoleAuthenticationConstants.CookieScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    AllowRefresh = false,
                    ExpiresUtc = clockService.UtcNow.Add(SessionLifetime),
                    IsPersistent = false
                });

            string validatedReturnUrl =
                WebConsoleRequestSecurity.ValidateLocalReturnUrl(
                    form["returnUrl"].ToString());
            return Results.LocalRedirect(validatedReturnUrl);
        }

        private static IResult BeginCloudLogin(
            [FromQuery] string? returnUrl,
            IOptions<WebConsoleOptions> options)
        {
            if (options.Value.Mode != WebConsoleMode.Cloud)
            {
                return Results.NotFound();
            }

            string validatedReturnUrl =
                WebConsoleRequestSecurity.ValidateLocalReturnUrl(returnUrl);
            return Results.Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = validatedReturnUrl
                },
                [
                    WebConsoleAuthenticationConstants
                        .CloudOpenIdConnectScheme
                ]);
        }

        private static async Task<IResult> GetSessionAsync(
            HttpContext context,
            IMediator mediator,
            IMapper mapper,
            IAntiforgery antiforgery,
            IOptions<WebConsoleOptions> options,
            CancellationToken cancellationToken)
        {
            string mode = options.Value.Mode
                .ToString()
                .ToLowerInvariant();
            if (!WebConsoleRequestSecurity.IsAllowed(
                    context,
                    options.Value))
            {
                return Results.StatusCode(
                    StatusCodes.Status403Forbidden);
            }

            AuthenticateResult authentication =
                await context.AuthenticateAsync(
                    WebConsoleAuthenticationConstants.CookieScheme);
            ClaimsPrincipal? principal = authentication.Principal;
            if (!authentication.Succeeded
                || principal?.Identity?.IsAuthenticated != true)
            {
                return Results.Ok(
                    new ConsoleSessionResponse(
                        false,
                        mode,
                        null,
                        [],
                        false));
            }

            string? issuer = principal.FindFirstValue(
                WebConsoleAuthenticationConstants.IdentityIssuerClaim);
            string? subject = principal.FindFirstValue(
                WebConsoleAuthenticationConstants.IdentitySubjectClaim);
            if (string.IsNullOrWhiteSpace(issuer)
                || string.IsNullOrWhiteSpace(subject))
            {
                await context.SignOutAsync(
                    WebConsoleAuthenticationConstants.CookieScheme);
                return Results.Ok(
                    new ConsoleSessionResponse(
                        false,
                        mode,
                        null,
                        [],
                        false));
            }

            context.User = principal;
            DomainResult<ListAccessibleWorkspacesResponse> result =
                await mediator.Send(
                    new ListAccessibleWorkspacesQuery(issuer, subject),
                    cancellationToken);
            if (result.IsFailure)
            {
                return Results.StatusCode(
                    StatusCodes.Status401Unauthorized);
            }

            IssueAntiforgeryToken(
                context,
                antiforgery,
                options.Value);
            string displayName =
                principal.FindFirstValue("name")
                ?? principal.FindFirstValue(ClaimTypes.Name)
                ?? subject;
            string? email =
                principal.FindFirstValue("email")
                ?? principal.FindFirstValue(ClaimTypes.Email);
            ConsoleWorkspaceResponse[] workspaces =
                mapper.Map<ConsoleWorkspaceResponse[]>(
                    result.Value.Items);

            return Results.Ok(
                new ConsoleSessionResponse(
                    true,
                    mode,
                    new ConsoleUserResponse(displayName, email),
                    workspaces,
                    false));
        }

        private static async Task<IResult> LogoutAsync(
            HttpContext context,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await context.SignOutAsync(
                WebConsoleAuthenticationConstants.CookieScheme);
            context.Response.Cookies.Delete(
                WebConsoleAuthenticationConstants
                    .AntiforgeryRequestCookieName);

            return Results.NoContent();
        }

        private static void IssueAntiforgeryToken(
            HttpContext context,
            IAntiforgery antiforgery,
            WebConsoleOptions options)
        {
            AntiforgeryTokenSet tokens =
                antiforgery.GetAndStoreTokens(context);
            if (string.IsNullOrWhiteSpace(tokens.RequestToken))
            {
                throw new InvalidOperationException(
                    "The Web Console antiforgery request token is unavailable.");
            }

            string cookieName = options.Mode == WebConsoleMode.Cloud
                ? "__Host-"
                  + WebConsoleAuthenticationConstants
                      .AntiforgeryRequestCookieName
                : WebConsoleAuthenticationConstants
                    .AntiforgeryRequestCookieName;
            context.Response.Cookies.Append(
                cookieName,
                tokens.RequestToken,
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.Add(
                        AntiforgeryLifetime),
                    HttpOnly = false,
                    IsEssential = true,
                    Path = "/",
                    SameSite = SameSiteMode.Strict,
                    Secure =
                        options.Mode == WebConsoleMode.Cloud
                        || context.Request.IsHttps
                });
        }
    }
}