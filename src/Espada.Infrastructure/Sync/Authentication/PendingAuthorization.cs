namespace Espada.Infrastructure.Sync.Authentication
{
    internal sealed record PendingAuthorization(
        string CodeVerifier,
        Uri RedirectUri,
        DateTimeOffset ExpiresAtUtc);
}