using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using OpenIddict.Validation.AspNetCore;

namespace Espada.Mcp.Security
{
    internal sealed class McpChallengeMetadataMiddleware(
        RequestDelegate next,
        IOptions<McpAuthorizationOptions> options)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.Equals(new PathString("/mcp")))
            {
                await next(context);
                return;
            }

            AuthenticateResult authentication =
                await context.AuthenticateAsync(
                    OpenIddictValidationAspNetCoreDefaults
                        .AuthenticationScheme);
            if (authentication.Succeeded
                && authentication.Principal is not null)
            {
                context.User = authentication.Principal;
                await next(context);
                return;
            }

            if (!context.Response.HasStarted)
            {
                Uri metadataUri = new(
                    options.Value.Resource,
                    "/.well-known/oauth-protected-resource/mcp");
                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;
                context.Response.Headers.WWWAuthenticate =
                    $"Bearer resource_metadata=\"{metadataUri.AbsoluteUri}\"";
            }
        }
    }
}