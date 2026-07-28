using OpenIddict.Abstractions;
using Espada.Mcp.Constants;

namespace Espada.Mcp.Security
{
    internal sealed class RefreshTokenReuseMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(
            HttpContext context,
            IOpenIddictTokenManager tokenManager,
            IOpenIddictAuthorizationManager authorizationManager)
        {
            if (context.Request.Method == HttpMethods.Post
                && context.Request.Path.Equals(
                    new PathString(
                        McpAuthorizationConstants.TokenEndpoint))
                && context.Request.HasFormContentType)
            {
                IFormCollection form = await context.Request.ReadFormAsync(
                    context.RequestAborted);
                if (form[OpenIddictConstants.Parameters.GrantType].ToString()
                        .Equals(
                            OpenIddictConstants.GrantTypes.RefreshToken,
                            StringComparison.Ordinal)
                    && !string.IsNullOrWhiteSpace(
                        form[OpenIddictConstants.Parameters.RefreshToken]))
                {
                    await RevokeReusedTokenFamilyAsync(
                        form[OpenIddictConstants.Parameters.RefreshToken]
                            .ToString(),
                        tokenManager,
                        authorizationManager,
                        context.RequestAborted);
                }
            }

            await next(context);
        }

        private static async Task RevokeReusedTokenFamilyAsync(
            string refreshToken,
            IOpenIddictTokenManager tokenManager,
            IOpenIddictAuthorizationManager authorizationManager,
            CancellationToken cancellationToken)
        {
            object? token = await tokenManager.FindByReferenceIdAsync(
                refreshToken,
                cancellationToken);
            if (token is null
                || !await tokenManager.HasStatusAsync(
                    token,
                    OpenIddictConstants.Statuses.Redeemed,
                    cancellationToken))
            {
                return;
            }

            string? authorizationId =
                await tokenManager.GetAuthorizationIdAsync(
                    token,
                    cancellationToken);
            if (string.IsNullOrWhiteSpace(authorizationId))
            {
                await tokenManager.TryRevokeAsync(
                    token,
                    cancellationToken);
                return;
            }

            await tokenManager.RevokeByAuthorizationIdAsync(
                authorizationId,
                cancellationToken);
            object? authorization =
                await authorizationManager.FindByIdAsync(
                    authorizationId,
                    cancellationToken);
            if (authorization is not null)
            {
                await authorizationManager.TryRevokeAsync(
                    authorization,
                    cancellationToken);
            }
        }
    }
}