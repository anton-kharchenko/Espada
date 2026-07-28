using Espada.Application.Policies;
using Espada.Domain.Rules;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using System.Security.Claims;
using Espada.Application.Constants;
using Espada.Mcp.Constants;
using Espada.Mcp.Models;
using Espada.Mcp.Security;

namespace Espada.Mcp.Services
{
    internal sealed class McpAuthorizationGrantService(
        WorkspaceAccessPolicy workspaceAccessPolicy,
        IOptions<McpAuthorizationOptions> options)
    {
        public async Task<McpAuthorizationGrant> CreateAsync(
            OpenIddictRequest request,
            ClaimsPrincipal localIdentity,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(localIdentity);

            string identityIssuer = GetRequiredClaim(
                localIdentity,
                McpAuthorizationConstants.IdentityIssuerClaim);
            string identitySubject = GetRequiredClaim(
                localIdentity,
                OpenIddictConstants.Claims.Subject);
            string clientId = request.ClientId
                              ?? throw new InvalidAuthorizationRequestException(
                                  OpenIddictConstants.Errors.InvalidRequest,
                                  "client_id is required.");
            string[] scopes = request.GetScopes()
                .Distinct(StringComparer.Ordinal)
                .Order()
                .ToArray();
            if (scopes.Length == 0)
            {
                throw new InvalidAuthorizationRequestException(
                    OpenIddictConstants.Errors.InvalidScope,
                    "At least one scope is required.");
            }

            string? unsupportedScope = scopes.FirstOrDefault(scope => !ApplicationScopeConstants.All.Contains(scope)
                                                                      && scope
                                                                      != McpAuthorizationConstants.OfflineAccessScope);
            if (unsupportedScope is not null)
            {
                throw new InvalidAuthorizationRequestException(
                    OpenIddictConstants.Errors.InvalidScope,
                    $"Scope '{unsupportedScope}' is not supported.");
            }

            Guid? workspaceId = ParseWorkspaceId(request);
            string[] applicationScopes = scopes
                .Where(scope =>
                    scope
                    != McpAuthorizationConstants.OfflineAccessScope)
                .ToArray();
            bool isWorkspaceCreation =
                applicationScopes.Contains(
                    ApplicationScopeConstants.WorkspaceCreate,
                    StringComparer.Ordinal);
            if (isWorkspaceCreation)
            {
                if (applicationScopes.Length != 1 || workspaceId.HasValue)
                {
                    throw new InvalidAuthorizationRequestException(
                        OpenIddictConstants.Errors.InvalidScope,
                        "workspace:create must be requested alone and without workspace_id.");
                }
            }
            else if (!workspaceId.HasValue)
            {
                throw new InvalidAuthorizationRequestException(
                    OpenIddictConstants.Errors.InvalidRequest,
                    "workspace_id is required for workspace access.");
            }
            else
            {
                DomainResult authorization =
                    await workspaceAccessPolicy.AuthorizeWorkspaceGrantAsync(
                        workspaceId.Value,
                        identityIssuer,
                        identitySubject,
                        cancellationToken);
                if (authorization.IsFailure)
                {
                    throw new InvalidAuthorizationRequestException(
                        OpenIddictConstants.Errors.AccessDenied,
                        "The identity is not authorized for this workspace.");
                }
            }

            return new McpAuthorizationGrant(
                identityIssuer,
                identitySubject,
                clientId,
                workspaceId,
                scopes,
                options.Value.DefaultRateCeilingPerMinute,
                options.Value.Resource.AbsoluteUri);
        }

        private static string GetRequiredClaim(
            ClaimsPrincipal principal,
            string claimType)
        {
            return principal.FindFirst(claimType)?.Value
                   ?? throw new InvalidAuthorizationRequestException(
                       OpenIddictConstants.Errors.AccessDenied,
                       "The local authority session is invalid.");
        }

        private static Guid? ParseWorkspaceId(OpenIddictRequest request)
        {
            string? value = request.GetParameter("workspace_id").ToString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Guid.TryParse(value, out Guid workspaceId)
                   && workspaceId != Guid.Empty
                ? workspaceId
                : throw new InvalidAuthorizationRequestException(
                    OpenIddictConstants.Errors.InvalidRequest,
                    "workspace_id must be a non-empty UUID.");
        }
    }
}