namespace Espada.Tests.Mcp.Http
{
    internal sealed record OAuthTokenResponse(
        string AccessToken,
        string RefreshToken,
        int ExpiresIn,
        string Scope);
}