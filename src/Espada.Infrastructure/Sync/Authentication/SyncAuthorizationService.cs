using Espada.Infrastructure.Sync.Contracts;
using Espada.Infrastructure.Sync.Options;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Espada.Infrastructure.Sync.Authentication
{
    internal sealed class SyncAuthorizationService(
        IHttpClientFactory httpClientFactory,
        SyncTokenStore tokenStore,
        IOptions<SyncClientOptions> options) : ISyncAuthorizationService
    {
        private static readonly TimeSpan AuthorizationLifetime = TimeSpan.FromMinutes(10);
        private readonly ConcurrentDictionary<string, PendingAuthorization> _pending = new(StringComparer.Ordinal);

        public Uri Begin(Uri redirectUri)
        {
            if (!options.Value.IsConfigured())
            {
                throw new InvalidOperationException("Espada Cloud sync is not configured.");
            }

            string state = Base64UrlText(RandomNumberGenerator.GetBytes(32));
            string verifier = Base64UrlText(RandomNumberGenerator.GetBytes(64));
            string challenge = Base64UrlText(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
            _pending[state] = new PendingAuthorization(verifier, redirectUri,
                DateTimeOffset.UtcNow.Add(AuthorizationLifetime));
            foreach ((string key, PendingAuthorization pending) in _pending)
            {
                if (pending.ExpiresAtUtc <= DateTimeOffset.UtcNow)
                {
                    _pending.TryRemove(key, out _);
                }
            }

            string endpoint = options.Value.Authority.TrimEnd('/') + "/oauth2/v2.0/authorize";
            string url = QueryHelpers.AddQueryString(endpoint, new Dictionary<string, string?>
            {
                ["client_id"] = options.Value.ClientId,
                ["response_type"] = "code",
                ["redirect_uri"] = redirectUri.ToString(),
                ["response_mode"] = "query",
                ["scope"] = $"openid profile offline_access {options.Value.Scope}",
                ["state"] = state,
                ["code_challenge"] = challenge,
                ["code_challenge_method"] = "S256"
            });
            return new Uri(url);
        }

        public async Task CompleteAsync(string state, string code, CancellationToken cancellationToken)
        {
            if (!_pending.TryRemove(state, out PendingAuthorization? pending)
                || pending.ExpiresAtUtc <= DateTimeOffset.UtcNow)
            {
                throw new InvalidOperationException("The authorization request is invalid or expired.");
            }

            SyncTokenSet token = await RequestTokenAsync(new Dictionary<string, string>
            {
                ["client_id"] = options.Value.ClientId,
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = pending.RedirectUri.ToString(),
                ["code_verifier"] = pending.CodeVerifier,
                ["scope"] = $"openid profile offline_access {options.Value.Scope}"
            }, null, cancellationToken);
            await tokenStore.WriteAsync(token, cancellationToken);
        }

        public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            SyncTokenSet? token = await tokenStore.ReadAsync(cancellationToken);
            if (token is null)
            {
                return null;
            }

            if (token.ExpiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(2))
            {
                return token.AccessToken;
            }

            if (string.IsNullOrWhiteSpace(token.RefreshToken))
            {
                return null;
            }

            SyncTokenSet refreshed = await RequestTokenAsync(new Dictionary<string, string>
            {
                ["client_id"] = options.Value.ClientId,
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = token.RefreshToken,
                ["scope"] = $"openid profile offline_access {options.Value.Scope}"
            }, token.RefreshToken, cancellationToken);
            await tokenStore.WriteAsync(refreshed, cancellationToken);
            return refreshed.AccessToken;
        }

        private async Task<SyncTokenSet> RequestTokenAsync(Dictionary<string, string> values,
            string? existingRefreshToken, CancellationToken cancellationToken)
        {
            using HttpRequestMessage request = new(HttpMethod.Post,
                options.Value.Authority.TrimEnd('/') + "/oauth2/v2.0/token")
            {
                Content = new FormUrlEncodedContent(values)
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using HttpResponseMessage response = await httpClientFactory.CreateClient()
                .SendAsync(request, cancellationToken);
            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Espada Cloud authorization failed with status {(int)response.StatusCode}.");
            }

            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;
            string accessToken = root.GetProperty("access_token").GetString()
                                 ?? throw new InvalidOperationException("The token response has no access token.");
            string? refreshToken = root.TryGetProperty("refresh_token", out JsonElement refresh)
                ? refresh.GetString()
                : existingRefreshToken;
            int expiresIn = root.TryGetProperty("expires_in", out JsonElement expires)
                ? expires.GetInt32()
                : 3600;
            return new SyncTokenSet(accessToken, refreshToken,
                DateTimeOffset.UtcNow.AddSeconds(Math.Max(60, expiresIn)));
        }

        private static string Base64UrlText(byte[] bytes)
        {
            return WebEncoders.Base64UrlEncode(bytes);
        }
    }
}