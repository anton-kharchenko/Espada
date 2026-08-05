using Espada.LocalSetup.Contracts.Requests;
using Espada.LocalSetup.Contracts.Responses;
using Espada.LocalSetup.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Espada.LocalSetup.Services
{
    internal sealed class ManagedMcpConfigurationWriter
    {
        private const string TomlStart = "# espada:managed:start";
        private const string TomlEnd = "# espada:managed:end";

        public async Task<IReadOnlyList<string>> WriteAsync(
            IReadOnlyList<LocalSetupMcpConfigurationPreview> configurations,
            IReadOnlySet<string> selectedAgents,
            CancellationToken cancellationToken)
        {
            List<string> configured = [];
            foreach (LocalSetupMcpConfigurationPreview configuration in configurations)
            {
                if (!selectedAgents.Contains(configuration.Agent))
                {
                    continue;
                }

                if (configuration.Agent.Equals("Codex", StringComparison.OrdinalIgnoreCase))
                {
                    await WriteTomlAsync(configuration.Path, cancellationToken);
                }
                else
                {
                    await WriteJsonAsync(configuration.Path, cancellationToken);
                }

                configured.Add(configuration.Agent);
            }

            return configured;
        }

        private static async Task WriteTomlAsync(string path, CancellationToken cancellationToken)
        {
            string block = $"{TomlStart}{Environment.NewLine}[mcp_servers.espada]{Environment.NewLine}"
                           + $"command = \"espada\"{Environment.NewLine}"
                           + $"args = [\"mcp\", \"stdio\"]{Environment.NewLine}{TomlEnd}";
            string content = File.Exists(path) ? await File.ReadAllTextAsync(path, cancellationToken) : string.Empty;
            int start = content.IndexOf(TomlStart, StringComparison.Ordinal);
            int end = content.IndexOf(TomlEnd, StringComparison.Ordinal);
            string updated = start >= 0 && end >= start
                ? content[..start] + block + content[(end + TomlEnd.Length)..]
                : string.IsNullOrWhiteSpace(content)
                    ? block + Environment.NewLine
                    : content.TrimEnd() + Environment.NewLine + Environment.NewLine + block + Environment.NewLine;
            if (string.Equals(content, updated, StringComparison.Ordinal))
            {
                return;
            }

            await WriteAtomicAsync(path, content, updated, cancellationToken);
        }

        private static async Task WriteJsonAsync(string path, CancellationToken cancellationToken)
        {
            string content = File.Exists(path) ? await File.ReadAllTextAsync(path, cancellationToken) : "{}";
            JsonObject root = JsonNode.Parse(content) as JsonObject
                ?? throw new InvalidOperationException($"MCP configuration root must be an object: {path}");
            JsonObject servers = root["mcpServers"] as JsonObject ?? new JsonObject();
            root["mcpServers"] = servers;
            servers["espada"] = new JsonObject
            {
                ["command"] = "espada",
                ["args"] = new JsonArray("mcp", "stdio")
            };
            string updated = root.ToJsonString(new JsonSerializerOptions { WriteIndented = true })
                             + Environment.NewLine;
            if (JsonNode.DeepEquals(JsonNode.Parse(content), root))
            {
                return;
            }

            await WriteAtomicAsync(path, content, updated, cancellationToken);
        }

        private static async Task WriteAtomicAsync(string path, string original, string updated,
            CancellationToken cancellationToken)
        {
            string directory = Path.GetDirectoryName(path)
                ?? throw new InvalidOperationException("MCP configuration directory is unavailable.");
            Directory.CreateDirectory(directory);
            if (File.Exists(path))
            {
                string backup = path + ".espada.backup";
                if (!File.Exists(backup))
                {
                    await File.WriteAllTextAsync(backup, original, cancellationToken);
                }
            }

            string temporary = path + ".espada.tmp";
            await File.WriteAllTextAsync(temporary, updated, cancellationToken);
            File.Move(temporary, path, true);
        }
    }
}