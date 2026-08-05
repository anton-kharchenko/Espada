namespace Espada.Mcp.Models
{
    internal sealed record McpAuthorizationGrant(
        string IdentityIssuer,
        string IdentitySubject,
        string ClientId,
        Guid? WorkspaceId,
        IReadOnlyList<string> Scopes,
        int RateCeilingPerMinute,
        string Resource);
}