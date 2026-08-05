using Espada.LocalSetup.Contracts.Requests;
using Espada.LocalSetup.Contracts.Responses;
using Espada.LocalSetup.Models;

namespace Espada.LocalSetup.Services
{
    internal sealed class McpConfigurationPreviewService
    {
        public IReadOnlyList<LocalSetupMcpConfigurationPreview> Create(
            IReadOnlyList<LocalSetupAgentPreview> agents)
        {
            string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            Dictionary<string, string> paths = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Codex"] = Path.Join(profile, ".codex", "config.toml"),
                ["Claude"] = Path.Join(profile, ".claude.json"),
                ["Gemini"] = Path.Join(profile, ".gemini", "settings.json"),
                ["Grok"] = Path.Join(profile, ".grok", "settings.json")
            };
            return agents.Where(agent => agent.IsInstalled)
                .Select(agent => new LocalSetupMcpConfigurationPreview(agent.Vendor, paths[agent.Vendor],
                    File.Exists(paths[agent.Vendor]) ? "Update managed espada entry" : "Create managed espada entry"))
                .ToArray();
        }
    }
}