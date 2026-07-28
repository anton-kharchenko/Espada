using Espada.Tests.Api.Fixtures;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Espada.Tests.Api.WebConsole
{
    public sealed class WebConsoleAuthenticationTests(
        WebConsoleApiFactory factory)
        : IClassFixture<WebConsoleApiFactory>
    {
        [Fact]
        public async Task Session_WithoutCookie_ShouldBeUnauthenticated()
        {
            using HttpClient client = factory.CreateConsoleClient();

            using HttpResponseMessage response = await client.GetAsync(
                "/bff/session",
                TestContext.Current.CancellationToken);

            response.EnsureSuccessStatusCode();
            using JsonDocument document = await ReadJsonAsync(response);
            Assert.False(
                document.RootElement
                    .GetProperty("authenticated")
                    .GetBoolean());
            Assert.Equal(
                "local",
                document.RootElement.GetProperty("mode").GetString());
        }

        [Fact]
        public async Task Bootstrap_ShouldCreateOneTimeHttpOnlySession()
        {
            Guid workspaceId = await factory.SeedWorkspaceAsync(
                "Shared memory",
                TestContext.Current.CancellationToken);
            using HttpClient client = factory.CreateConsoleClient();
            (string code, string returnUrl) =
                await CreateBootstrapCodeAsync(client);
            using FormUrlEncodedContent firstForm =
                CreateBootstrapForm(code, returnUrl);

            using HttpResponseMessage firstResponse =
                await client.PostAsync(
                    "/bff/auth/bootstrap",
                    firstForm,
                    TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Redirect, firstResponse.StatusCode);
            Assert.Equal(
                "/app",
                firstResponse.Headers.Location?.OriginalString);
            string sessionCookie = Assert.Single(
                firstResponse.Headers
                    .GetValues("Set-Cookie"),
                value => value.StartsWith(
                    "Espada.Console.Session=",
                    StringComparison.Ordinal));
            Assert.Contains(
                "httponly",
                sessionCookie,
                StringComparison.OrdinalIgnoreCase);

            using HttpResponseMessage sessionResponse =
                await client.GetAsync(
                    "/bff/session",
                    TestContext.Current.CancellationToken);
            sessionResponse.EnsureSuccessStatusCode();
            using JsonDocument session = await ReadJsonAsync(sessionResponse);
            JsonElement root = session.RootElement;
            Assert.True(root.GetProperty("authenticated").GetBoolean());
            Assert.Equal(
                "test-user",
                root.GetProperty("user")
                    .GetProperty("displayName")
                    .GetString());
            JsonElement workspace = Assert.Single(
                root.GetProperty("workspaces")
                    .EnumerateArray()
                    .Where(item =>
                        item.GetProperty("id").GetGuid() == workspaceId));
            Assert.Equal(
                workspaceId,
                workspace.GetProperty("id").GetGuid());
            Assert.Equal(
                "Shared memory",
                workspace.GetProperty("name").GetString());
            Assert.Contains(
                sessionResponse.Headers.GetValues("Set-Cookie"),
                value => value.StartsWith(
                    "Espada.Console.Csrf=",
                    StringComparison.Ordinal));

            using FormUrlEncodedContent secondForm =
                CreateBootstrapForm(code, returnUrl);
            using HttpResponseMessage secondResponse =
                await factory.CreateConsoleClient().PostAsync(
                    "/bff/auth/bootstrap",
                    secondForm,
                    TestContext.Current.CancellationToken);
            Assert.Equal(
                HttpStatusCode.BadRequest,
                secondResponse.StatusCode);
        }

        [Fact]
        public async Task Logout_ShouldRequireSameOriginAndAntiforgeryPair()
        {
            using HttpClient client = factory.CreateConsoleClient();
            (string code, string returnUrl) =
                await CreateBootstrapCodeAsync(client);
            using FormUrlEncodedContent form =
                CreateBootstrapForm(code, returnUrl);
            using HttpResponseMessage bootstrap =
                await client.PostAsync(
                    "/bff/auth/bootstrap",
                    form,
                    TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.Redirect, bootstrap.StatusCode);
            using HttpResponseMessage session = await client.GetAsync(
                "/bff/session",
                TestContext.Current.CancellationToken);
            string antiforgeryToken = GetCookieValue(
                session,
                "Espada.Console.Csrf");

            using HttpResponseMessage missingOrigin =
                await client.PostAsync(
                    "/bff/session/logout",
                    null,
                    TestContext.Current.CancellationToken);
            Assert.Equal(
                HttpStatusCode.Forbidden,
                missingOrigin.StatusCode);

            using HttpRequestMessage missingToken =
                new(HttpMethod.Post, "/bff/session/logout");
            missingToken.Headers.Add("Origin", "https://localhost");
            using HttpResponseMessage missingTokenResponse =
                await client.SendAsync(
                    missingToken,
                    TestContext.Current.CancellationToken);
            Assert.Equal(
                HttpStatusCode.BadRequest,
                missingTokenResponse.StatusCode);

            using HttpRequestMessage validRequest =
                new(HttpMethod.Post, "/bff/session/logout");
            validRequest.Headers.Add("Origin", "https://localhost");
            validRequest.Headers.Add("X-CSRF-TOKEN", antiforgeryToken);
            using HttpResponseMessage validResponse =
                await client.SendAsync(
                    validRequest,
                    TestContext.Current.CancellationToken);
            Assert.Equal(
                HttpStatusCode.NoContent,
                validResponse.StatusCode);
        }

        [Fact]
        public async Task Logout_ThroughSameOriginProxy_ShouldUseForwardedOrigin()
        {
            using HttpClient client = factory.CreateConsoleClient();
            (string code, string returnUrl) =
                await CreateBootstrapCodeAsync(client);
            using FormUrlEncodedContent form =
                CreateBootstrapForm(code, returnUrl);
            using HttpResponseMessage bootstrap =
                await client.PostAsync(
                    "/bff/auth/bootstrap",
                    form,
                    TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.Redirect, bootstrap.StatusCode);
            using HttpResponseMessage session = await client.GetAsync(
                "/bff/session",
                TestContext.Current.CancellationToken);
            string antiforgeryToken = GetCookieValue(
                session,
                "Espada.Console.Csrf");

            using HttpRequestMessage request =
                new(HttpMethod.Post, "/bff/session/logout");
            request.Headers.Add(
                "Origin",
                "http://localhost:59182");
            request.Headers.Add(
                "X-Forwarded-For",
                "127.0.0.1");
            request.Headers.Add(
                "X-Forwarded-Host",
                "localhost:59182");
            request.Headers.Add(
                "X-Forwarded-Proto",
                "http");
            request.Headers.Add(
                "X-CSRF-TOKEN",
                antiforgeryToken);

            using HttpResponseMessage response = await client.SendAsync(
                request,
                TestContext.Current.CancellationToken);

            Assert.Equal(
                HttpStatusCode.NoContent,
                response.StatusCode);
        }

        [Fact]
        public async Task NonConsoleRequest_ThroughHttpProxy_ShouldRedirectToHttps()
        {
            using HttpClient client = factory.CreateConsoleClient();
            using HttpRequestMessage request =
                new(HttpMethod.Get, "/health");
            request.Headers.Add(
                "X-Forwarded-For",
                "127.0.0.1");
            request.Headers.Add(
                "X-Forwarded-Host",
                "localhost:59182");
            request.Headers.Add(
                "X-Forwarded-Proto",
                "http");

            using HttpResponseMessage response = await client.SendAsync(
                request,
                TestContext.Current.CancellationToken);

            Assert.Equal(
                HttpStatusCode.TemporaryRedirect,
                response.StatusCode);
            Assert.Equal(
                "https://localhost:7180/health",
                response.Headers.Location?.OriginalString);
        }

        [Fact]
        public async Task BootstrapLink_FromNonLoopbackHost_ShouldBeForbidden()
        {
            using HttpClient client = factory.CreateConsoleClient();
            using HttpRequestMessage request =
                new(HttpMethod.Post, "/bff/auth/bootstrap-link");
            request.Headers.Host = "192.0.2.1";

            using HttpResponseMessage response = await client.SendAsync(
                request,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task WorkspaceEndpoints_ShouldEnforceTenantCsrfAndOwnerPolicy()
        {
            Guid workspaceId = await factory.SeedWorkspaceAsync(
                $"Accessible {Guid.NewGuid():N}",
                TestContext.Current.CancellationToken);
            Guid inaccessibleWorkspaceId =
                await factory.SeedWorkspaceAsync(
                    $"Inaccessible {Guid.NewGuid():N}",
                    TestContext.Current.CancellationToken,
                    grantAccess: false);
            using HttpClient client = factory.CreateConsoleClient();
            (string code, string returnUrl) =
                await CreateBootstrapCodeAsync(client);
            using FormUrlEncodedContent form =
                CreateBootstrapForm(code, returnUrl);
            using HttpResponseMessage bootstrap =
                await client.PostAsync(
                    "/bff/auth/bootstrap",
                    form,
                    TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.Redirect, bootstrap.StatusCode);
            using HttpResponseMessage session = await client.GetAsync(
                "/bff/session",
                TestContext.Current.CancellationToken);
            string antiforgeryToken = GetCookieValue(
                session,
                "Espada.Console.Csrf");

            using HttpResponseMessage accessible =
                await client.GetAsync(
                    $"/bff/workspaces/{workspaceId:D}/projects",
                    TestContext.Current.CancellationToken);
            Assert.True(
                accessible.IsSuccessStatusCode,
                await accessible.Content.ReadAsStringAsync(
                    TestContext.Current.CancellationToken));
            using HttpResponseMessage inaccessible =
                await client.GetAsync(
                    $"/bff/workspaces/{inaccessibleWorkspaceId:D}/projects",
                    TestContext.Current.CancellationToken);
            Assert.Equal(
                HttpStatusCode.Forbidden,
                inaccessible.StatusCode);

            using HttpRequestMessage createProject = CreateMutation(
                HttpMethod.Post,
                $"/bff/workspaces/{workspaceId:D}/projects",
                new
                {
                    name = "Espada",
                    canonicalRemoteUri =
                        "https://github.com/example/espada.git",
                    localAliases = new[] { @"C:\Startups\Espada" }
                },
                antiforgeryToken);
            using HttpResponseMessage projectResponse =
                await client.SendAsync(
                    createProject,
                    TestContext.Current.CancellationToken);
            Assert.Equal(
                HttpStatusCode.Created,
                projectResponse.StatusCode);

            using HttpRequestMessage blockedPolicy = CreateMutation(
                HttpMethod.Post,
                $"/bff/workspaces/{workspaceId:D}/artifacts",
                new
                {
                    title = "Blocked policy",
                    typeId = 2,
                    content = "Do not expose secrets.",
                    kindTypeId = 3,
                    policyRules = new[]
                    {
                        new
                        {
                            ruleKey = "security.secrets",
                            text = "Do not expose secrets.",
                            priority = 100,
                            enforcementTypeId = 1
                        }
                    }
                },
                antiforgeryToken);
            using HttpResponseMessage blockedPolicyResponse =
                await client.SendAsync(
                    blockedPolicy,
                    TestContext.Current.CancellationToken);
            Assert.Equal(
                HttpStatusCode.Forbidden,
                blockedPolicyResponse.StatusCode);

            using HttpRequestMessage ownerPolicy = CreateMutation(
                HttpMethod.Post,
                $"/bff/workspaces/{workspaceId:D}/policies",
                new
                {
                    title = "Security",
                    typeId = 2,
                    content = "Do not expose secrets.",
                    kindTypeId = 1,
                    policyRules = new[]
                    {
                        new
                        {
                            ruleKey = "security.secrets",
                            text = "Do not expose secrets.",
                            priority = 100,
                            enforcementTypeId = 1
                        }
                    }
                },
                antiforgeryToken);
            using HttpResponseMessage ownerPolicyResponse =
                await client.SendAsync(
                    ownerPolicy,
                    TestContext.Current.CancellationToken);
            Assert.Equal(
                HttpStatusCode.Created,
                ownerPolicyResponse.StatusCode);
        }

        private static async Task<(string Code, string ReturnUrl)>
            CreateBootstrapCodeAsync(HttpClient client)
        {
            using HttpResponseMessage response =
                await client.PostAsync(
                    "/bff/auth/bootstrap-link",
                    null,
                    TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();
            using JsonDocument document = await ReadJsonAsync(response);
            string url =
                document.RootElement.GetProperty("url").GetString()
                ?? throw new InvalidOperationException(
                    "The bootstrap URL is missing.");
            Assert.StartsWith(
                "/bff/auth/bootstrap#",
                url,
                StringComparison.Ordinal);
            Dictionary<string, string> values = url[
                    (url.IndexOf('#', StringComparison.Ordinal) + 1)..]
                .TrimStart('#')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split('=', 2))
                .ToDictionary(
                    part => Uri.UnescapeDataString(part[0]),
                    part => Uri.UnescapeDataString(part[1]));

            return (values["code"], values["returnUrl"]);
        }

        private static FormUrlEncodedContent CreateBootstrapForm(
            string code,
            string returnUrl)
        {
            return new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["code"] = code,
                    ["returnUrl"] = returnUrl
                });
        }

        private static HttpRequestMessage CreateMutation(
            HttpMethod method,
            string path,
            object body,
            string antiforgeryToken)
        {
            HttpRequestMessage request = new(method, path)
            {
                Content = JsonContent.Create(body)
            };
            request.Headers.Add("Origin", "https://localhost");
            request.Headers.Add("X-CSRF-TOKEN", antiforgeryToken);
            return request;
        }

        private static string GetCookieValue(
            HttpResponseMessage response,
            string cookieName)
        {
            string cookie = Assert.Single(
                response.Headers
                    .GetValues("Set-Cookie"),
                value => value.StartsWith(
                    $"{cookieName}=",
                    StringComparison.Ordinal));
            int separator = cookie.IndexOf(';');
            string value = separator < 0
                ? cookie
                : cookie[..separator];
            return Uri.UnescapeDataString(
                value[(cookieName.Length + 1)..]);
        }

        private static async Task<JsonDocument> ReadJsonAsync(
            HttpResponseMessage response)
        {
            return await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(
                    TestContext.Current.CancellationToken),
                cancellationToken:
                TestContext.Current.CancellationToken);
        }
    }
}
