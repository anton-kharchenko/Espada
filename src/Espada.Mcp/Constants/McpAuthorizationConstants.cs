namespace Espada.Mcp.Constants
{
    internal static class McpAuthorizationConstants
    {
        public const string SectionName = "Mcp:Authorization";
        public const string AuthorityCookieScheme = "Espada.Mcp.Authority";
        public const string EntraScheme = "Espada.Mcp.Entra";
        public const string AccessPolicy = "Espada.Mcp.Access";
        public const string RateLimitPolicy = "Espada.Mcp.RateLimit";

        public const string RegistrationRateLimitPolicy =
            "Espada.Mcp.RegistrationRateLimit";

        public const string ResourceName = "Espada MCP";
        public const string OfflineAccessScope = "offline_access";
        public const string AuthorizationEndpoint = "/connect/authorize";
        public const string TokenEndpoint = "/connect/token";
        public const string RevocationEndpoint = "/connect/revoke";
        public const string RegistrationEndpoint = "/connect/register";
        public const string BootstrapEndpoint = "/auth/bootstrap";
        public const string BootstrapLinkEndpoint = "/auth/bootstrap-links";
        public const string IdentityIssuerClaim = "espada_identity_issuer";
        public const string ClientIdentityClaim = "espada_client_id";
        public const string WorkspaceIdClaim = "espada_workspace_id";
        public const string RateCeilingClaim = "espada_rate_ceiling";
        public const string AntiforgeryFieldName = "__RequestVerificationToken";
        public const int AuthorizationCodeLifetimeMinutes = 5;
        public const int AccessTokenLifetimeMinutes = 15;
        public const int RefreshTokenLifetimeDays = 30;
    }
}