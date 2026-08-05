using Espada.Api.Authentication.Constants;
using Espada.Application.Constants;
using Espada.Application.Contracts.Security;
using Espada.Application.Models;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Espada.Api.Authentication
{
    internal sealed class ApiRequestPrincipalAccessor(
        IHttpContextAccessor httpContextAccessor,
        IOptions<WebConsoleOptions> options)
        : IRequestPrincipalAccessor
    {
        public RequestPrincipal? Principal
        {
            get
            {
                ClaimsPrincipal? user =
                    httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true
                    || user.Identity.AuthenticationType
                    != WebConsoleAuthenticationConstants.CookieScheme)
                {
                    return null;
                }

                string? issuer = user.FindFirstValue(
                    WebConsoleAuthenticationConstants
                        .IdentityIssuerClaim);
                string? subject = user.FindFirstValue(
                    WebConsoleAuthenticationConstants
                        .IdentitySubjectClaim);
                if (string.IsNullOrWhiteSpace(issuer)
                    || string.IsNullOrWhiteSpace(subject))
                {
                    return null;
                }

                return new RequestPrincipal(
                    issuer,
                    subject,
                    "espada:web-console",
                    null,
                    ApplicationScopeConstants.All,
                    60,
                    options.Value.Mode == WebConsoleMode.Local);
            }
        }
    }
}