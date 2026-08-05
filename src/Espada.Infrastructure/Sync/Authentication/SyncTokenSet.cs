namespace Espada.Infrastructure.Sync.Authentication
{
    internal sealed record SyncTokenSet(
        string AccessToken,
        string? RefreshToken,
        DateTimeOffset ExpiresAtUtc);
}