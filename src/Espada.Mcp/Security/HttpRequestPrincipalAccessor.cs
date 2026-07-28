using Espada.Application.Constants;
using Espada.Application.Contracts.Security;
using Espada.Application.Models;
using Espada.Mcp.Constants;
using OpenIddict.Abstractions;
using System.Collections.Frozen;
using System.Security.Claims;

namespace Espada.Mcp.Security
{
    internal sealed class HttpRequestPrincipalAccessor(
        IHttpContextAccessor httpContextAccessor)
        : IRequestPrincipalAccessor
    {
        public RequestPrincipal? Principal
        {
            get
            {
                ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                {
                    return null;
                }

                string? identityIssuer =
                    user.FindFirstValue(
                        McpAuthorizationConstants.IdentityIssuerClaim);
                string? identitySubject =
                    user.FindFirstValue(OpenIddictConstants.Claims.Subject);
                string? clientId =
                    user.FindFirstValue(
                        McpAuthorizationConstants.ClientIdentityClaim);
                string? workspaceIdValue =
                    user.FindFirstValue(
                        McpAuthorizationConstants.WorkspaceIdClaim);
                string? rateCeilingValue =
                    user.FindFirstValue(
                        McpAuthorizationConstants.RateCeilingClaim);

                if (string.IsNullOrWhiteSpace(identityIssuer)
                    || string.IsNullOrWhiteSpace(identitySubject)
                    || string.IsNullOrWhiteSpace(clientId)
                    || !int.TryParse(rateCeilingValue, out int rateCeiling)
                    || rateCeiling <= 0)
                {
                    return null;
                }

                Guid? workspaceId = Guid.TryParse(
                                        workspaceIdValue,
                                        out Guid parsedWorkspaceId)
                                    && parsedWorkspaceId != Guid.Empty
                    ? parsedWorkspaceId
                    : null;
                FrozenSet<string> scopes = user.GetScopes()
                    .Where(ApplicationScopeConstants.All.Contains)
                    .ToFrozenSet(StringComparer.Ordinal);

                return new RequestPrincipal(
                    identityIssuer,
                    identitySubject,
                    clientId,
                    workspaceId,
                    scopes,
                    rateCeiling,
                    false);
            }
        }
    }
}