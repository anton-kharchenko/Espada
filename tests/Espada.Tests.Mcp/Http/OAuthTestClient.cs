using Espada.Application.Constants;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Espada.Tests.Mcp.Http
{
    internal sealed class OAuthTestClient(
        McpFactory factory,
        HttpClient client)
    {
        private const string RedirectUri =
            "http://127.0.0.1:49152/callback";

        private static readonly Regex AntiforgeryTokenPattern = new(
            "name=\"__RequestVerificationToken\"\\s+value=\"([^\"]+)\"",
            RegexOptions.CultureInvariant);

        public async Task<string> RegisterClientAsync(
            CancellationToken cancellationToken)
        {
            using HttpResponseMessage response = await client.PostAsJsonAsync(
                "/connect/register",
                new
                {
                    client_name = "Espada MCP tests",
                    redirect_uris = new[] { RedirectUri },
                    token_endpoint_auth_method = "none",
                    grant_types = new[] { "authorization_code", "refresh_token" },
                    response_types = new[] { "code" },
                    scope = string.Join(
                        ' ',
                        ApplicationScopeConstants.All
                            .Append("offline_access")
                            .Order())
                },
                cancellationToken);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                string body = await response.Content.ReadAsStringAsync(
                    cancellationToken);
                Assert.Fail(
                    $"DCR returned {(int)response.StatusCode}: {body}");
            }

            using JsonDocument document =
                await JsonDocument.ParseAsync(
                    await response.Content.ReadAsStreamAsync(
                        cancellationToken),
                    cancellationToken: cancellationToken);
            Assert.False(
                document.RootElement.TryGetProperty(
                    "client_secret",
                    out _));
            return document.RootElement
                       .GetProperty("client_id")
                       .GetString()
                   ?? throw new InvalidOperationException(
                       "DCR response did not contain client_id.");
        }

        public async Task AuthenticateAuthorityAsync(
            CancellationToken cancellationToken)
        {
            string code =
                await factory.CreateAuthorityBootstrapCodeAsync(
                    cancellationToken);
            using HttpResponseMessage response = await client.PostAsync(
                "/auth/bootstrap",
                new FormUrlEncodedContent(
                    new Dictionary<string, string> { ["code"] = code }),
                cancellationToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public Task<OAuthTokenResponse> AuthorizeWorkspaceCreationAsync(
            string clientId,
            CancellationToken cancellationToken)
        {
            return AuthorizeAsync(
                clientId,
                [ApplicationScopeConstants.WorkspaceCreate],
                null,
                null,
                cancellationToken);
        }

        public Task<OAuthTokenResponse> AuthorizeWorkspaceCreationAsync(
            string clientId,
            Func<CancellationToken, Task> authorizationCodeIssued,
            CancellationToken cancellationToken)
        {
            return AuthorizeAsync(
                clientId,
                [ApplicationScopeConstants.WorkspaceCreate],
                null,
                authorizationCodeIssued,
                cancellationToken);
        }

        public Task<OAuthTokenResponse> AuthorizeWorkspaceAsync(
            string clientId,
            Guid workspaceId,
            IReadOnlyCollection<string> scopes,
            CancellationToken cancellationToken)
        {
            return AuthorizeAsync(
                clientId,
                scopes,
                workspaceId,
                null,
                cancellationToken);
        }

        public async Task<OAuthTokenResponse> RefreshAsync(
            string clientId,
            string refreshToken,
            CancellationToken cancellationToken)
        {
            using HttpResponseMessage response = await client.PostAsync(
                "/connect/token",
                new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        ["grant_type"] = "refresh_token",
                        ["client_id"] = clientId,
                        ["refresh_token"] = refreshToken
                    }),
                cancellationToken);
            response.EnsureSuccessStatusCode();
            return await ReadTokenResponseAsync(
                response,
                cancellationToken);
        }

        public Task<HttpResponseMessage> ReuseRefreshTokenAsync(
            string clientId,
            string refreshToken,
            CancellationToken cancellationToken)
        {
            return client.PostAsync(
                "/connect/token",
                new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        ["grant_type"] = "refresh_token",
                        ["client_id"] = clientId,
                        ["refresh_token"] = refreshToken
                    }),
                cancellationToken);
        }

        public Task<HttpResponseMessage> RevokeAsync(
            string clientId,
            string token,
            CancellationToken cancellationToken)
        {
            return client.PostAsync(
                "/connect/revoke",
                new FormUrlEncodedContent(
                    new Dictionary<string, string> { ["client_id"] = clientId, ["token"] = token }),
                cancellationToken);
        }

        private async Task<OAuthTokenResponse> AuthorizeAsync(
            string clientId,
            IReadOnlyCollection<string> scopes,
            Guid? workspaceId,
            Func<CancellationToken, Task>? authorizationCodeIssued,
            CancellationToken cancellationToken)
        {
            string[] requestedScopes = scopes
                .Append("offline_access")
                .Distinct(StringComparer.Ordinal)
                .Order()
                .ToArray();
            string verifier = WebEncoders.Base64UrlEncode(
                RandomNumberGenerator.GetBytes(32));
            string challenge = WebEncoders.Base64UrlEncode(
                SHA256.HashData(
                    Encoding.ASCII.GetBytes(verifier)));
            Dictionary<string, string?> parameters = new()
            {
                ["client_id"] = clientId,
                ["redirect_uri"] = RedirectUri,
                ["response_type"] = "code",
                ["scope"] = string.Join(' ', requestedScopes),
                ["code_challenge"] = challenge,
                ["code_challenge_method"] = "S256"
            };
            if (workspaceId.HasValue)
            {
                parameters["workspace_id"] =
                    workspaceId.Value.ToString("D");
            }

            string authorizationUri = QueryHelpers.AddQueryString(
                "/connect/authorize",
                parameters);
            using HttpResponseMessage consentResponse =
                await client.GetAsync(
                    authorizationUri,
                    cancellationToken);
            Assert.Equal(HttpStatusCode.OK, consentResponse.StatusCode);
            string consentHtml =
                await consentResponse.Content.ReadAsStringAsync(
                    cancellationToken);
            Match antiforgeryMatch =
                AntiforgeryTokenPattern.Match(consentHtml);
            Assert.True(antiforgeryMatch.Success);

            Dictionary<string, string> authorizationForm = new()
            {
                ["decision"] = "allow",
                ["__RequestVerificationToken"] =
                    WebUtility.HtmlDecode(
                        antiforgeryMatch.Groups[1].Value),
                ["client_id"] = clientId,
                ["redirect_uri"] = RedirectUri,
                ["response_type"] = "code",
                ["scope"] = string.Join(' ', requestedScopes),
                ["code_challenge"] = challenge,
                ["code_challenge_method"] = "S256"
            };
            if (workspaceId.HasValue)
            {
                authorizationForm["workspace_id"] =
                    workspaceId.Value.ToString("D");
            }

            using HttpResponseMessage authorizationResponse =
                await client.PostAsync(
                    authorizationUri,
                    new FormUrlEncodedContent(
                        authorizationForm),
                    cancellationToken);
            if (authorizationResponse.StatusCode
                != HttpStatusCode.Redirect)
            {
                string body =
                    await authorizationResponse.Content.ReadAsStringAsync(
                        cancellationToken);
                Assert.Fail(
                    $"Authorization returned {(int)authorizationResponse.StatusCode}: {body}");
            }

            Uri redirect = authorizationResponse.Headers.Location
                           ?? throw new InvalidOperationException(
                               "Authorization response did not contain a redirect.");
            Dictionary<string, StringValues>
                query = QueryHelpers.ParseQuery(redirect.Query);
            string authorizationCode = query["code"].ToString();
            Assert.False(string.IsNullOrWhiteSpace(authorizationCode));
            if (authorizationCodeIssued is not null)
            {
                await authorizationCodeIssued(cancellationToken);
            }

            using HttpResponseMessage tokenResponse =
                await client.PostAsync(
                    "/connect/token",
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            ["grant_type"] = "authorization_code",
                            ["client_id"] = clientId,
                            ["code"] = authorizationCode,
                            ["redirect_uri"] = RedirectUri,
                            ["code_verifier"] = verifier
                        }),
                    cancellationToken);
            tokenResponse.EnsureSuccessStatusCode();
            return await ReadTokenResponseAsync(
                tokenResponse,
                cancellationToken);
        }

        private static async Task<OAuthTokenResponse> ReadTokenResponseAsync(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            using JsonDocument document =
                await JsonDocument.ParseAsync(
                    await response.Content.ReadAsStreamAsync(
                        cancellationToken),
                    cancellationToken: cancellationToken);
            JsonElement root = document.RootElement;
            return new OAuthTokenResponse(
                root.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException(
                    "Token response did not contain access_token."),
                root.GetProperty("refresh_token").GetString()
                ?? throw new InvalidOperationException(
                    "Token response did not contain refresh_token."),
                root.GetProperty("expires_in").GetInt32(),
                root.GetProperty("scope").GetString() ?? string.Empty);
        }
    }
}