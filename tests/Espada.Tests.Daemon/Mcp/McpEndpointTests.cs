using Espada.Comms.Core.Security;
using Espada.Protocol.Mcp.Contracts.Requests;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Espada.Tests.Daemon.Mcp;

public sealed class McpEndpointTests
{
    [Fact]
    public async Task Mcp_ShouldRequireApiKeyAndExposeContextSearch()
    {
        ContextSearchServiceStub service = new();
        await using DaemonFactory factory = new(service);
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage unauthorized = await client.PostAsJsonAsync(
            "/mcp",
            new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "initialize",
                @params = new
                {
                    protocolVersion = McpTestValues.ProtocolVersion,
                    capabilities = new { },
                    clientInfo = new { name = McpTestValues.ClientName, version = McpTestValues.ClientVersion }
                }
            },
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        client.DefaultRequestHeaders.Add(ApiKeyAuthenticationDefaults.DefaultHeaderName, McpTestValues.ApiKey);
        HttpClientTransportOptions options = new()
        {
            Endpoint = new Uri(client.BaseAddress!, "/mcp"),
            Name = "Espada daemon tests"
        };
        await using HttpClientTransport transport = new(options, client, loggerFactory: null, ownsHttpClient: false);
        await using McpClient mcpClient = await McpClient.CreateAsync(transport, cancellationToken: TestContext.Current.CancellationToken);

        IList<McpClientTool> tools = await mcpClient.ListToolsAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.Contains(tools, tool => tool.Name == McpTestValues.ContextSearchToolName);

        ContextSearchRequest request = new(Guid.NewGuid(), McpTestValues.QueryText, McpTestValues.ModelIdentifier, McpTestValues.ModelVersion);
        CallToolResult result = await mcpClient.CallToolAsync(McpTestValues.ContextSearchToolName, new Dictionary<string, object?> { ["request"] = request }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(result.IsError ?? false);
        Assert.Equal(request, service.ReceivedRequest);
    }
}