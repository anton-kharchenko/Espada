using Espada.Application.Constants;
using Espada.Application.Contracts.Security;
using Espada.Application.Models;
using System.Collections.Frozen;

namespace Espada.Mcp.Security
{
    internal sealed class TrustedLocalRequestPrincipalAccessor
        : IRequestPrincipalAccessor
    {
        private const string SectionName = "Mcp:TrustedLocal";

        public TrustedLocalRequestPrincipalAccessor(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            string clientId = configuration[$"{SectionName}:ClientId"]
                              ?? "local-mcp";
            string identityIssuer = configuration[$"{SectionName}:IdentityIssuer"]
                                    ?? "espada:local";
            string identitySubject =
                configuration[$"{SectionName}:IdentitySubject"]
                ?? Environment.UserName;
            int rateCeiling = configuration
                .GetValue($"{SectionName}:RateCeilingPerMinute", 60);
            Guid? workspaceId = ParseWorkspaceId(
                configuration[$"{SectionName}:WorkspaceId"]);
            IReadOnlySet<string> scopes = ParseScopes(
                configuration[$"{SectionName}:Scopes"],
                workspaceId);

            if (string.IsNullOrWhiteSpace(clientId)
                || string.IsNullOrWhiteSpace(identityIssuer)
                || string.IsNullOrWhiteSpace(identitySubject)
                || rateCeiling <= 0)
            {
                throw new InvalidOperationException(
                    "Trusted local MCP principal configuration is invalid.");
            }

            Principal = new RequestPrincipal(
                identityIssuer.Trim(),
                identitySubject.Trim(),
                clientId.Trim(),
                workspaceId,
                scopes,
                rateCeiling,
                true);
        }

        public RequestPrincipal Principal { get; }

        private static Guid? ParseWorkspaceId(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Guid.TryParse(value, out Guid workspaceId)
                   && workspaceId != Guid.Empty
                ? workspaceId
                : throw new InvalidOperationException(
                    "Mcp:TrustedLocal:WorkspaceId must be a non-empty UUID.");
        }

        private static IReadOnlySet<string> ParseScopes(
            string? value,
            Guid? workspaceId)
        {
            IEnumerable<string> scopes = string.IsNullOrWhiteSpace(value)
                ? workspaceId.HasValue
                    ? ApplicationScopeConstants.All.Where(scope => scope != ApplicationScopeConstants.WorkspaceCreate)
                    : [ApplicationScopeConstants.WorkspaceCreate]
                : value.Split(
                    [',', ' '],
                    StringSplitOptions.RemoveEmptyEntries
                    | StringSplitOptions.TrimEntries);
            FrozenSet<string> parsedScopes = scopes.ToFrozenSet(
                StringComparer.Ordinal);
            string? unsupportedScope = parsedScopes.FirstOrDefault(scope => !ApplicationScopeConstants.All.Contains(scope));
            if (unsupportedScope is not null)
            {
                throw new InvalidOperationException(
                    $"MCP scope '{unsupportedScope}' is not supported.");
            }

            return parsedScopes;
        }
    }
}