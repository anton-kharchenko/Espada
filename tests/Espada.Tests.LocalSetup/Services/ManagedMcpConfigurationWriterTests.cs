using Espada.LocalSetup.Contracts.Responses;
using Espada.LocalSetup.Services;
using System.Text.Json.Nodes;
using Xunit;

namespace Espada.Tests.LocalSetup.Services
{
    public sealed class ManagedMcpConfigurationWriterTests
    {
        [Fact]
        public async Task WriteAsync_ShouldPreserveExistingEntriesBackupAndRemainIdempotent()
        {
            string root = Path.Join(Path.GetTempPath(), "espada-local-setup-tests", Guid.NewGuid().ToString("N"));
            string codexPath = Path.Join(root, "codex", "config.toml");
            string geminiPath = Path.Join(root, "gemini", "settings.json");
            Directory.CreateDirectory(Path.GetDirectoryName(codexPath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(geminiPath)!);
            string originalToml = $"[existing]{Environment.NewLine}value = 42{Environment.NewLine}";
            string originalJson = """{"mcpServers":{"existing":{"command":"existing"}},"theme":"dark"}""";

            try
            {
                await File.WriteAllTextAsync(codexPath, originalToml, TestContext.Current.CancellationToken);
                await File.WriteAllTextAsync(geminiPath, originalJson, TestContext.Current.CancellationToken);
                LocalSetupMcpConfigurationPreview[] configurations =
                [
                    new("Codex", codexPath, "Update"),
                    new("Gemini", geminiPath, "Update")
                ];
                HashSet<string> selectedAgents = new(StringComparer.OrdinalIgnoreCase) { "Codex", "Gemini" };
                ManagedMcpConfigurationWriter writer = new();

                IReadOnlyList<string> configured = await writer.WriteAsync(
                    configurations, selectedAgents, TestContext.Current.CancellationToken);
                string toml = await File.ReadAllTextAsync(codexPath, TestContext.Current.CancellationToken);
                string json = await File.ReadAllTextAsync(geminiPath, TestContext.Current.CancellationToken);

                Assert.Equal(["Codex", "Gemini"], configured);
                Assert.Contains("[existing]", toml, StringComparison.Ordinal);
                Assert.Equal(1, toml.Split("# espada:managed:start", StringSplitOptions.None).Length - 1);
                Assert.Equal(originalToml, await File.ReadAllTextAsync(
                    codexPath + ".espada.backup", TestContext.Current.CancellationToken));
                JsonObject rootNode = Assert.IsType<JsonObject>(JsonNode.Parse(json));
                Assert.Equal("dark", rootNode["theme"]?.GetValue<string>());
                Assert.Equal("existing", rootNode["mcpServers"]?["existing"]?["command"]?.GetValue<string>());
                Assert.Equal("espada", rootNode["mcpServers"]?["espada"]?["command"]?.GetValue<string>());
                Assert.Equal(originalJson, await File.ReadAllTextAsync(
                    geminiPath + ".espada.backup", TestContext.Current.CancellationToken));

                await writer.WriteAsync(configurations, selectedAgents, TestContext.Current.CancellationToken);

                Assert.Equal(toml, await File.ReadAllTextAsync(codexPath, TestContext.Current.CancellationToken));
                Assert.Equal(json, await File.ReadAllTextAsync(geminiPath, TestContext.Current.CancellationToken));
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            }
        }
    }
}