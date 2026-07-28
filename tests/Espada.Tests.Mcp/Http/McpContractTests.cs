using ModelContextProtocol.Client;
using System.Net;
using System.Net.Http.Headers;

namespace Espada.Tests.Mcp.Http;

public sealed class McpContractTests(McpFactory factory)
    : IClassFixture<McpFactory>
{
    private static readonly string[] ExpectedTools =
    [
        "artifact.create",
        "artifact.get",
        "artifact.list",
        "artifact.revise",
        "binding.remove",
        "binding.set",
        "context.build",
        "memory.remember",
        "memory.search",
        "source.import",
        "source.register",
        "workspace.create",
        "workspace.get"
    ];

    private static readonly string[] ExpectedResourceTemplates =
    [
        "artifact://{id}",
        "artifact://{id}/revision/{number}",
        "chunk://{id}",
        "workspace://{id}/instructions",
        "workspace://{id}/memory"
    ];

    [Fact]
    public async Task HttpContract_ShouldExposeOnlyCanonicalToolsAndResources()
    {
        using HttpClient httpClient = CreateClient(factory);
        OAuthTestClient oauth = new(factory, httpClient);
        string clientId = await oauth.RegisterClientAsync(
            TestContext.Current.CancellationToken);
        await oauth.AuthenticateAuthorityAsync(
            TestContext.Current.CancellationToken);
        OAuthTokenResponse token =
            await oauth.AuthorizeWorkspaceCreationAsync(
                clientId,
                TestContext.Current.CancellationToken);
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token.AccessToken);
        await using HttpClientTransport transport = CreateTransport(httpClient);
        await using McpClient client = await McpClient.CreateAsync(
            transport,
            cancellationToken: TestContext.Current.CancellationToken);

        IList<McpClientTool> tools = await client.ListToolsAsync(
            cancellationToken: TestContext.Current.CancellationToken);
        IList<McpClientResourceTemplate> templates =
            await client.ListResourceTemplatesAsync(
                cancellationToken: TestContext.Current.CancellationToken);
        _ = await client.ListResourcesAsync(
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            ExpectedTools,
            tools.Select(tool => tool.Name).Order().ToArray());
        Assert.Equal(
            ExpectedResourceTemplates,
            templates
                .Select(template => template.UriTemplate)
                .Order()
                .ToArray());
        Assert.DoesNotContain(tools, tool => tool.Name == "context.search");
        Assert.All(tools, tool =>
        {
            Assert.NotEqual(
                System.Text.Json.JsonValueKind.Undefined,
                tool.JsonSchema.ValueKind);
            Assert.True(tool.ReturnJsonSchema.HasValue);
            Assert.NotEqual(
                System.Text.Json.JsonValueKind.Undefined,
                tool.ReturnJsonSchema.Value.ValueKind);
        });
    }

    [Fact]
    public async Task HttpEndpoint_WithoutBearer_ShouldReturnUnauthorized()
    {
        using HttpClient httpClient = CreateClient(factory);
        using HttpResponseMessage response = await httpClient.PostAsync(
            "/mcp",
            new StringContent(
                "{}",
                System.Text.Encoding.UTF8,
                "application/json"),
            TestContext.Current.CancellationToken);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
        Assert.Contains(
            response.Headers.WwwAuthenticate,
            value => value.Parameter?.Contains(
                "resource_metadata=",
                StringComparison.Ordinal) == true);
    }

    [Fact]
    public async Task HttpHost_ShouldNotExposeLegacySseEndpoint()
    {
        using HttpClient client = CreateClient(factory);

        using HttpResponseMessage response = await client.GetAsync(
            "/sse",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static HttpClientTransport CreateTransport(
        HttpClient httpClient) =>
        new(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                Name = "Espada MCP contract tests",
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient,
            loggerFactory: null,
            ownsHttpClient: false);

    private static HttpClient CreateClient(McpFactory factory) =>
        factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing
                .WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });
}
