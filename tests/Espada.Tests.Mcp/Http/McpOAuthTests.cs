using Espada.Application.Constants;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenIddict.Abstractions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Espada.Tests.Mcp.Http
{
    public sealed class McpOAuthTests(McpFactory factory)
        : IClassFixture<McpFactory>
    {
        [Fact]
        public async Task Discovery_ShouldExposeOAuth21PkceAndProtectedResourceMetadata()
        {
            using HttpClient client = factory.CreateOAuthClient();

            using HttpResponseMessage discoveryResponse =
                await client.GetAsync(
                    "/.well-known/oauth-authorization-server",
                    TestContext.Current.CancellationToken);
            discoveryResponse.EnsureSuccessStatusCode();
            using JsonDocument discovery =
                await JsonDocument.ParseAsync(
                    await discoveryResponse.Content.ReadAsStreamAsync(
                        TestContext.Current.CancellationToken),
                    cancellationToken:
                    TestContext.Current.CancellationToken);
            JsonElement root = discovery.RootElement;
            Assert.Equal(
                "http://localhost/connect/authorize",
                root.GetProperty("authorization_endpoint").GetString());
            Assert.Equal(
                "http://localhost/connect/token",
                root.GetProperty("token_endpoint").GetString());
            Assert.Equal(
                "http://localhost/connect/revoke",
                root.GetProperty("revocation_endpoint").GetString());
            Assert.Equal(
                ["S256"],
                root.GetProperty("code_challenge_methods_supported")
                    .EnumerateArray()
                    .Select(value => value.GetString())
                    .OfType<string>()
                    .ToArray());
            string?[] grantTypes = root
                .GetProperty("grant_types_supported")
                .EnumerateArray()
                .Select(value => value.GetString())
                .ToArray();
            Assert.Contains("authorization_code", grantTypes);
            Assert.Contains("refresh_token", grantTypes);
            Assert.DoesNotContain("implicit", grantTypes);
            Assert.DoesNotContain("password", grantTypes);
            Assert.DoesNotContain("client_credentials", grantTypes);

            using HttpResponseMessage metadataResponse =
                await client.GetAsync(
                    "/.well-known/oauth-protected-resource/mcp",
                    TestContext.Current.CancellationToken);
            metadataResponse.EnsureSuccessStatusCode();
            using JsonDocument metadata =
                await JsonDocument.ParseAsync(
                    await metadataResponse.Content.ReadAsStreamAsync(
                        TestContext.Current.CancellationToken),
                    cancellationToken:
                    TestContext.Current.CancellationToken);
            Assert.Equal(
                "http://localhost/mcp",
                metadata.RootElement
                    .GetProperty("resource")
                    .GetString());
            Assert.Contains(
                "http://localhost/",
                metadata.RootElement
                    .GetProperty("authorization_servers")
                    .EnumerateArray()
                    .Select(value => value.GetString()));
        }

        [Fact]
        public async Task DynamicClientRegistration_ShouldRejectNonLoopbackHttpRedirectAndSecrets()
        {
            using HttpClient client = factory.CreateOAuthClient();

            using HttpResponseMessage redirectResponse =
                await client.PostAsJsonAsync(
                    "/connect/register",
                    new
                    {
                        client_name = "Invalid redirect",
                        redirect_uris = new[] { "http://example.com/callback" },
                        token_endpoint_auth_method = "none"
                    },
                    TestContext.Current.CancellationToken);
            Assert.Equal(
                HttpStatusCode.BadRequest,
                redirectResponse.StatusCode);

            using HttpResponseMessage secretResponse =
                await client.PostAsJsonAsync(
                    "/connect/register",
                    new
                    {
                        client_name = "Confidential client",
                        redirect_uris = new[] { "https://client.example/callback" },
                        token_endpoint_auth_method =
                            "client_secret_basic"
                    },
                    TestContext.Current.CancellationToken);
            Assert.Equal(
                HttpStatusCode.BadRequest,
                secretResponse.StatusCode);
        }

        [Fact]
        public async Task BootstrapCode_ShouldBeOneTime()
        {
            using HttpClient firstClient = factory.CreateOAuthClient();
            using HttpClient secondClient = factory.CreateOAuthClient();
            string code =
                await factory.CreateAuthorityBootstrapCodeAsync(
                    TestContext.Current.CancellationToken);
            using FormUrlEncodedContent form = new(
                new Dictionary<string, string> { ["code"] = code });

            using HttpResponseMessage firstResponse =
                await firstClient.PostAsync(
                    "/auth/bootstrap",
                    form,
                    TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

            using HttpResponseMessage secondResponse =
                await secondClient.PostAsync(
                    "/auth/bootstrap",
                    new FormUrlEncodedContent(
                        new Dictionary<string, string> { ["code"] = code }),
                    TestContext.Current.CancellationToken);
            Assert.Equal(
                HttpStatusCode.BadRequest,
                secondResponse.StatusCode);
        }

        [Fact]
        public async Task AuthorizationCode_ShouldRequirePkceS256AndIssueExpectedLifetimes()
        {
            using HttpClient client = factory.CreateOAuthClient();
            OAuthTestClient oauth = new(factory, client);
            string clientId = await oauth.RegisterClientAsync(
                TestContext.Current.CancellationToken);
            await oauth.AuthenticateAuthorityAsync(
                TestContext.Current.CancellationToken);

            using HttpResponseMessage missingPkceResponse =
                await client.GetAsync(
                    $"/connect/authorize?client_id={clientId}"
                    + "&redirect_uri=http%3A%2F%2F127.0.0.1%3A49152%2Fcallback"
                    + "&response_type=code"
                    + "&scope=workspace%3Acreate%20offline_access",
                    TestContext.Current.CancellationToken);
            Assert.Equal(
                HttpStatusCode.BadRequest,
                missingPkceResponse.StatusCode);

            TimeSpan? authorizationCodeLifetime = null;
            OAuthTokenResponse token =
                await oauth.AuthorizeWorkspaceCreationAsync(
                    clientId,
                    async cancellationToken =>
                        authorizationCodeLifetime =
                            await factory.GetLatestTokenLifetimeAsync(
                                OpenIddictConstants.GrantTypes.AuthorizationCode,
                                cancellationToken),
                    TestContext.Current.CancellationToken);
            Assert.InRange(token.ExpiresIn, 899, 900);
            Assert.Contains(
                ApplicationScopeConstants.WorkspaceCreate,
                token.Scope.Split(' '));
            Assert.Contains("offline_access", token.Scope.Split(' '));
            Assert.Equal(
                TimeSpan.FromMinutes(5),
                authorizationCodeLifetime);
            Assert.Equal(
                TimeSpan.FromMinutes(15),
                await factory.GetLatestTokenLifetimeAsync(
                    OpenIddictConstants.TokenTypeHints.AccessToken,
                    TestContext.Current.CancellationToken));
            Assert.Equal(
                TimeSpan.FromDays(30),
                await factory.GetLatestTokenLifetimeAsync(
                    OpenIddictConstants.TokenTypeHints.RefreshToken,
                    TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task RefreshTokenReuse_ShouldRevokeTokenFamily()
        {
            using HttpClient client = factory.CreateOAuthClient();
            OAuthTestClient oauth = new(factory, client);
            string clientId = await oauth.RegisterClientAsync(
                TestContext.Current.CancellationToken);
            await oauth.AuthenticateAuthorityAsync(
                TestContext.Current.CancellationToken);
            OAuthTokenResponse first =
                await oauth.AuthorizeWorkspaceCreationAsync(
                    clientId,
                    TestContext.Current.CancellationToken);
            OAuthTokenResponse second = await oauth.RefreshAsync(
                clientId,
                first.RefreshToken,
                TestContext.Current.CancellationToken);
            Assert.NotEqual(
                first.RefreshToken,
                second.RefreshToken);

            using HttpResponseMessage reuseResponse =
                await oauth.ReuseRefreshTokenAsync(
                    clientId,
                    first.RefreshToken,
                    TestContext.Current.CancellationToken);
            Assert.Equal(
                HttpStatusCode.BadRequest,
                reuseResponse.StatusCode);

            using HttpResponseMessage familyResponse =
                await oauth.ReuseRefreshTokenAsync(
                    clientId,
                    second.RefreshToken,
                    TestContext.Current.CancellationToken);
            Assert.Equal(
                HttpStatusCode.BadRequest,
                familyResponse.StatusCode);

            using HttpResponseMessage protectedResponse =
                await SendMcpRequestAsync(
                    client,
                    second.AccessToken,
                    null);
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                protectedResponse.StatusCode);
        }

        [Fact]
        public async Task Revocation_ShouldInvalidateAccessToken()
        {
            using HttpClient client = factory.CreateOAuthClient();
            OAuthTestClient oauth = new(factory, client);
            string clientId = await oauth.RegisterClientAsync(
                TestContext.Current.CancellationToken);
            await oauth.AuthenticateAuthorityAsync(
                TestContext.Current.CancellationToken);
            OAuthTokenResponse token =
                await oauth.AuthorizeWorkspaceCreationAsync(
                    clientId,
                    TestContext.Current.CancellationToken);

            using HttpResponseMessage beforeRevocation =
                await SendMcpRequestAsync(
                    client,
                    token.AccessToken,
                    null);
            Assert.NotEqual(
                HttpStatusCode.Unauthorized,
                beforeRevocation.StatusCode);

            using HttpResponseMessage revocationResponse =
                await oauth.RevokeAsync(
                    clientId,
                    token.AccessToken,
                    TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, revocationResponse.StatusCode);

            using HttpResponseMessage afterRevocation =
                await SendMcpRequestAsync(
                    client,
                    token.AccessToken,
                    null);
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                afterRevocation.StatusCode);
        }

        [Fact]
        public async Task McpEndpoint_ShouldRejectUntrustedOrigin()
        {
            using HttpClient client = factory.CreateOAuthClient();
            using HttpResponseMessage response = await SendMcpRequestAsync(
                client,
                null,
                "https://evil.example");

            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task McpRateLimit_ShouldEnforceTokenBurstPerClient()
        {
            using HttpClient client = factory.CreateOAuthClient();
            OAuthTestClient oauth = new(factory, client);
            string clientId = await oauth.RegisterClientAsync(
                TestContext.Current.CancellationToken);
            await oauth.AuthenticateAuthorityAsync(
                TestContext.Current.CancellationToken);
            OAuthTokenResponse token =
                await oauth.AuthorizeWorkspaceCreationAsync(
                    clientId,
                    TestContext.Current.CancellationToken);

            Task<HttpResponseMessage>[] requests = Enumerable
                .Range(0, 12)
                .Select(_ => SendMcpRequestAsync(
                    client,
                    token.AccessToken,
                    null))
                .ToArray();
            HttpResponseMessage[] responses =
                await Task.WhenAll(requests);
            try
            {
                Assert.Contains(
                    responses,
                    response => response.StatusCode
                                == HttpStatusCode.TooManyRequests);
            }
            finally
            {
                foreach (HttpResponseMessage response in responses)
                {
                    response.Dispose();
                }
            }
        }

        [Fact]
        public async Task WorkspaceTokens_ShouldEnforceBootstrapScopeAndWorkspaceBoundary()
        {
            using HttpClient httpClient = factory.CreateOAuthClient();
            OAuthTestClient oauth = new(factory, httpClient);
            string clientId = await oauth.RegisterClientAsync(
                TestContext.Current.CancellationToken);
            await oauth.AuthenticateAuthorityAsync(
                TestContext.Current.CancellationToken);
            OAuthTokenResponse bootstrapToken =
                await oauth.AuthorizeWorkspaceCreationAsync(
                    clientId,
                    TestContext.Current.CancellationToken);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    bootstrapToken.AccessToken);

            Guid firstWorkspaceId;
            Guid secondWorkspaceId;
            await using (HttpClientTransport bootstrapTransport =
                         CreateTransport(httpClient))
            await using (McpClient bootstrapClient =
                         await McpClient.CreateAsync(
                             bootstrapTransport,
                             cancellationToken:
                             TestContext.Current.CancellationToken))
            {
                firstWorkspaceId = await CreateWorkspaceAsync(
                    bootstrapClient,
                    "OAuth workspace one");
                secondWorkspaceId = await CreateWorkspaceAsync(
                    bootstrapClient,
                    "OAuth workspace two");
            }

            OAuthTokenResponse workspaceToken =
                await oauth.AuthorizeWorkspaceAsync(
                    clientId,
                    firstWorkspaceId,
                    [ApplicationScopeConstants.WorkspaceRead],
                    TestContext.Current.CancellationToken);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    workspaceToken.AccessToken);
            await using (HttpClientTransport workspaceTransport =
                         CreateTransport(httpClient))
            await using (McpClient workspaceClient =
                         await McpClient.CreateAsync(
                             workspaceTransport,
                             cancellationToken:
                             TestContext.Current.CancellationToken))
            {
                CallToolResult ownWorkspace = await workspaceClient.CallToolAsync(
                    "workspace.get",
                    WorkspaceGetArguments(firstWorkspaceId),
                    cancellationToken:
                    TestContext.Current.CancellationToken);
                Assert.False(ownWorkspace.IsError is true);
                Assert.Equal(
                    firstWorkspaceId,
                    ownWorkspace.StructuredContent?
                        .GetProperty("id")
                        .GetGuid());

                CallToolResult otherWorkspace = await workspaceClient.CallToolAsync(
                    "workspace.get",
                    WorkspaceGetArguments(secondWorkspaceId),
                    cancellationToken:
                    TestContext.Current.CancellationToken);
                Assert.True(otherWorkspace.IsError);
                Assert.Contains(
                    "forbidden",
                    GetErrorText(otherWorkspace),
                    StringComparison.Ordinal);
            }

            OAuthTokenResponse missingScopeToken =
                await oauth.AuthorizeWorkspaceAsync(
                    clientId,
                    firstWorkspaceId,
                    [ApplicationScopeConstants.ContextRead],
                    TestContext.Current.CancellationToken);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    missingScopeToken.AccessToken);
            await using HttpClientTransport missingScopeTransport =
                CreateTransport(httpClient);
            await using McpClient missingScopeClient =
                await McpClient.CreateAsync(
                    missingScopeTransport,
                    cancellationToken:
                    TestContext.Current.CancellationToken);

            CallToolResult missingScope = await missingScopeClient.CallToolAsync(
                "workspace.get",
                WorkspaceGetArguments(firstWorkspaceId),
                cancellationToken:
                TestContext.Current.CancellationToken);
            Assert.True(missingScope.IsError);
            Assert.Contains(
                "forbidden",
                GetErrorText(missingScope),
                StringComparison.Ordinal);
        }

        private static async Task<Guid> CreateWorkspaceAsync(
            McpClient client,
            string name)
        {
            CallToolResult result = await client.CallToolAsync(
                "workspace.create",
                new Dictionary<string, object?>
                {
                    ["request"] = new Dictionary<string, object?> { ["name"] = name, ["typeId"] = 1 }
                },
                cancellationToken: TestContext.Current.CancellationToken);
            Assert.False(result.IsError is true);

            return result.StructuredContent?
                       .GetProperty("workspaceId")
                       .GetGuid()
                   ?? throw new InvalidOperationException(
                       "workspace.create did not return workspaceId.");
        }

        private static Dictionary<string, object?> WorkspaceGetArguments(
            Guid workspaceId)
        {
            return new Dictionary<string, object?>
            {
                ["request"] = new Dictionary<string, object?> { ["workspaceId"] = workspaceId }
            };
        }

        private static string GetErrorText(CallToolResult result)
        {
            return result.Content
                       .OfType<TextContentBlock>()
                       .FirstOrDefault()?
                       .Text
                   ?? string.Empty;
        }

        private static HttpClientTransport CreateTransport(
            HttpClient httpClient)
        {
            return new HttpClientTransport(
                new HttpClientTransportOptions
                {
                    Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                    Name = "Espada MCP authorization tests",
                    TransportMode = HttpTransportMode.StreamableHttp
                },
                httpClient,
                null,
                false);
        }

        private static Task<HttpResponseMessage> SendMcpRequestAsync(
            HttpClient client,
            string? accessToken,
            string? origin)
        {
            HttpRequestMessage request = new(HttpMethod.Post, "/mcp")
            {
                Content = new StringContent(
                    "{}",
                    Encoding.UTF8,
                    "application/json")
            };
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        accessToken);
            }

            if (!string.IsNullOrWhiteSpace(origin))
            {
                request.Headers.Add("Origin", origin);
            }

            return client.SendAsync(
                request,
                TestContext.Current.CancellationToken);
        }
    }
}