using Espada.Api.LocalSetup.Models;
using Espada.Domain.Enums;

namespace Espada.Api.LocalSetup.Services
{
    internal sealed class AgentDiscoveryService
    {
        public async Task<IReadOnlyList<LocalSetupAgentPreview>> DiscoverAsync(
            CancellationToken cancellationToken)
        {
            (AgentVendorType Vendor, string Command)[] candidates =
            [
                (AgentVendorType.Codex, "codex"),
                (AgentVendorType.Claude, "claude"),
                (AgentVendorType.Gemini, "gemini"),
                (AgentVendorType.Grok, "grok")
            ];
            List<LocalSetupAgentPreview> agents = [];
            foreach ((AgentVendorType vendor, string command) in candidates)
            {
                string? executable = FindExecutable(command);
                if (executable is null)
                {
                    agents.Add(new LocalSetupAgentPreview(vendor.Id, vendor.Name, false, false, null, null));
                    continue;
                }

                LocalProcessResult version = await LocalProcess.RunAsync(executable, ["--version"], null,
                    TimeSpan.FromSeconds(5), cancellationToken);
                bool authenticated = await IsAuthenticatedAsync(vendor, executable, cancellationToken);
                agents.Add(new LocalSetupAgentPreview(vendor.Id, vendor.Name, true, authenticated, executable,
                    version.Succeeded ? version.StandardOutput.Trim() : null));
            }

            return agents;
        }

        private static async Task<bool> IsAuthenticatedAsync(AgentVendorType vendor, string executable,
            CancellationToken cancellationToken)
        {
            if (vendor.Equals(AgentVendorType.Gemini))
            {
                return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GEMINI_API_KEY"))
                       || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GOOGLE_API_KEY"))
                       || File.Exists(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                           ".gemini", "oauth_creds.json"));
            }

            IReadOnlyList<string> arguments = vendor.Equals(AgentVendorType.Codex)
                ? ["login", "status"]
                : ["auth", "status"];
            LocalProcessResult result = await LocalProcess.RunAsync(executable, arguments, null,
                TimeSpan.FromSeconds(5), cancellationToken);
            return result.Succeeded;
        }

        private static string? FindExecutable(string command)
        {
            string? pathValue = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrWhiteSpace(pathValue))
            {
                return null;
            }

            string[] extensions = OperatingSystem.IsWindows()
                ? (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT")
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                : [string.Empty];
            foreach (string directory in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                foreach (string extension in extensions)
                {
                    string candidate = Path.Join(directory.Trim(), command + extension.ToLowerInvariant());
                    if (File.Exists(candidate))
                    {
                        return Path.GetFullPath(candidate);
                    }
                }
            }

            return null;
        }
    }
}
