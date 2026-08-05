namespace Espada.Application.Models
{
    public sealed record RequestPrincipal(
        string IdentityIssuer,
        string IdentitySubject,
        string ClientId,
        Guid? WorkspaceId,
        IReadOnlySet<string> Scopes,
        int RateCeilingPerMinute,
        bool IsTrustedLocalTransport)
    {
        public bool HasScope(string scope)
        {
            return Scopes.Contains(scope);
        }
    }
}