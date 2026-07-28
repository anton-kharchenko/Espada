using Espada.Application.Constants;
using Espada.Tests.Mcp.Http;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.Text.Json;

namespace Espada.Tests.Mcp.Stdio
{
    public sealed class AgentCompatibilitySmokeTests(McpFactory factory)
        : IClassFixture<McpFactory>
    {
        private const string IdentityIssuer = "espada:test";
        private const string IdentitySubject = "shared-agent-user";

        [Fact]
        public async Task CodexClaudeAndGemini_ShouldReadTheSameCanonicalMemory()
        {
            await using McpClient bootstrapClient = await CreateClientAsync(
                "bootstrap",
                null,
                ApplicationScopeConstants.WorkspaceCreate);
            Guid workspaceId = await CreateWorkspaceAsync(bootstrapClient);

            string[] agents =
            [
                ContextAgentConstants.Codex,
                ContextAgentConstants.Claude,
                ContextAgentConstants.Gemini
            ];
            foreach (string agent in agents)
            {
                await using McpClient client = await CreateClientAsync(
                    agent,
                    workspaceId,
                    WorkspaceScopes());
                await RememberAsync(client, workspaceId, agent);
            }

            foreach (string agent in agents)
            {
                await using McpClient client = await CreateClientAsync(
                    agent,
                    workspaceId,
                    WorkspaceScopes());
                CallToolResult search = await client.CallToolAsync(
                    "memory.search",
                    Request(new Dictionary<string, object?>
                    {
                        ["workspaceId"] = workspaceId,
                        ["queryText"] = "canonical shared memory",
                        ["topK"] = 10
                    }),
                    cancellationToken:
                    TestContext.Current.CancellationToken);
                Assert.False(search.IsError is true);
                JsonElement items = search.StructuredContent?
                                        .GetProperty("items")
                                    ?? throw new InvalidOperationException(
                                        "memory.search did not return items.");
                Assert.Equal(agents.Length, items.GetArrayLength());
                foreach (string expectedAgent in agents)
                {
                    JsonElement item = Assert.Single(
                        items.EnumerateArray().Where(candidate =>
                            candidate
                                .GetProperty("provenance")
                                .GetProperty("clientIdentity")
                                .GetString()
                            == expectedAgent));
                    Assert.Equal(
                        MemoryContent(expectedAgent),
                        item.GetProperty("content").GetString());
                }

                CallToolResult context = await client.CallToolAsync(
                    "context.build",
                    Request(new Dictionary<string, object?>
                    {
                        ["workspaceId"] = workspaceId,
                        ["projectId"] = null,
                        ["taskId"] = null,
                        ["repositoryRelativePath"] = null,
                        ["branch"] = null,
                        ["agent"] = agent,
                        ["tokenBudget"] = 4096
                    }),
                    cancellationToken:
                    TestContext.Current.CancellationToken);
                Assert.False(context.IsError is true);
                JsonElement projection = context.StructuredContent?
                                             .GetProperty("projection")
                                         ?? throw new InvalidOperationException(
                                             "context.build did not return a projection.");
                Assert.Equal(
                    agent,
                    projection.GetProperty("agent").GetString());
                string projectionContent =
                    projection.GetProperty("content").GetString()
                    ?? string.Empty;
                foreach (string expectedAgent in agents)
                {
                    Assert.Contains(
                        MemoryContent(expectedAgent),
                        projectionContent,
                        StringComparison.Ordinal);
                }
            }
        }

        private async Task<McpClient> CreateClientAsync(
            string clientId,
            Guid? workspaceId,
            string scopes)
        {
            Dictionary<string, string?> environment =
                StdioClientTransportOptions.GetDefaultEnvironmentVariables();
            environment["ConnectionStrings__Espada"] =
                factory.ConnectionString;
            environment["DOTNET_ENVIRONMENT"] = "Testing";
            environment["Mcp__TrustedLocal__ClientId"] = clientId;
            environment["Mcp__TrustedLocal__IdentityIssuer"] =
                IdentityIssuer;
            environment["Mcp__TrustedLocal__IdentitySubject"] =
                IdentitySubject;
            environment["Mcp__TrustedLocal__Scopes"] = scopes;
            environment["Mcp__TrustedLocal__WorkspaceId"] =
                workspaceId?.ToString();

            StdioClientTransport transport = new(
                new StdioClientTransportOptions
                {
                    Name = $"Espada {clientId} compatibility smoke",
                    Command = "dotnet",
                    Arguments =
                    [
                        Path.Join(
                            AppContext.BaseDirectory,
                            "Espada.Mcp.dll"),
                        "stdio"
                    ],
                    WorkingDirectory = AppContext.BaseDirectory,
                    InheritEnvironmentVariables = false,
                    EnvironmentVariables = environment,
                    ShutdownTimeout = TimeSpan.FromSeconds(5)
                });
            return await McpClient.CreateAsync(
                transport,
                cancellationToken:
                TestContext.Current.CancellationToken);
        }

        private static async Task<Guid> CreateWorkspaceAsync(
            McpClient client)
        {
            CallToolResult result = await client.CallToolAsync(
                "workspace.create",
                Request(new Dictionary<string, object?>
                {
                    ["name"] = "Shared agent workspace",
                    ["typeId"] = 1
                }),
                cancellationToken:
                TestContext.Current.CancellationToken);
            Assert.False(result.IsError is true);

            return result.StructuredContent?
                       .GetProperty("workspaceId")
                       .GetGuid()
                   ?? throw new InvalidOperationException(
                       "workspace.create did not return workspaceId.");
        }

        private static async Task RememberAsync(
            McpClient client,
            Guid workspaceId,
            string agent)
        {
            CallToolResult result = await client.CallToolAsync(
                "memory.remember",
                Request(new Dictionary<string, object?>
                {
                    ["workspaceId"] = workspaceId,
                    ["title"] = $"Shared memory from {agent}",
                    ["content"] = MemoryContent(agent),
                    ["categoryTypeId"] = 2,
                    ["confidence"] = 0.92m,
                    ["sessionIdentity"] =
                        $"compatibility-smoke-{agent}"
                }),
                cancellationToken:
                TestContext.Current.CancellationToken);
            Assert.False(result.IsError is true);
            Assert.False(
                result.StructuredContent?
                    .GetProperty("userConfirmed")
                    .GetBoolean());
        }

        private static string MemoryContent(string agent)
        {
            return $"Canonical shared memory recorded by {agent}.";
        }

        private static Dictionary<string, object?> Request(
            Dictionary<string, object?> request)
        {
            return new Dictionary<string, object?>
            {
                ["request"] = request
            };
        }

        private static string WorkspaceScopes()
        {
            return string.Join(
                ' ',
                ApplicationScopeConstants.All.Where(scope =>
                    scope
                    != ApplicationScopeConstants.WorkspaceCreate));
        }
    }
}
