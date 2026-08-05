using Espada.LocalSetup.Contracts.Requests;
using Espada.LocalSetup.Contracts.Responses;
using Espada.LocalSetup.Models;
using System.Security.Cryptography;
using System.Text;

namespace Espada.LocalSetup.Services
{
    internal sealed class GitRepositoryInspector
    {
        private const int MaximumInstructionSizeBytes = 1_048_576;

        public async Task<GitRepositorySnapshot> InspectAsync(string path, CancellationToken cancellationToken)
        {
            string candidate = Path.GetFullPath(path);
            if (!Directory.Exists(candidate))
            {
                throw new DirectoryNotFoundException($"Repository path does not exist: {candidate}");
            }

            LocalProcessResult rootResult = await LocalProcess.RunAsync("git",
                ["rev-parse", "--show-toplevel"], candidate, TimeSpan.FromSeconds(10), cancellationToken);
            if (!rootResult.Succeeded || string.IsNullOrWhiteSpace(rootResult.StandardOutput))
            {
                throw new InvalidOperationException("The selected directory is not inside a Git repository.");
            }

            string root = Path.GetFullPath(rootResult.StandardOutput.Trim());
            LocalProcessResult remoteResult = await LocalProcess.RunAsync("git",
                ["config", "--get", "remote.origin.url"], root, TimeSpan.FromSeconds(10), cancellationToken);
            string? remote = remoteResult.Succeeded && !string.IsNullOrWhiteSpace(remoteResult.StandardOutput)
                ? remoteResult.StandardOutput.Trim()
                : null;
            LocalProcessResult filesResult = await LocalProcess.RunAsync("git",
                ["ls-files", "-z", "--", "AGENTS.md", "CLAUDE.md", "GEMINI.md",
                    ":(glob)**/AGENTS.md", ":(glob)**/CLAUDE.md", ":(glob)**/GEMINI.md"],
                root, TimeSpan.FromSeconds(10), cancellationToken);
            if (!filesResult.Succeeded)
            {
                throw new InvalidOperationException("Git could not enumerate tracked instruction files.");
            }

            List<LocalSetupInstructionPreview> instructions = [];
            foreach (string relativePath in filesResult.StandardOutput.Split('\0',
                         StringSplitOptions.RemoveEmptyEntries))
            {
                string fullPath = Path.GetFullPath(Path.Join(root, relativePath));
                if (!IsInside(root, fullPath) || !File.Exists(fullPath) || IsEscapingLink(root, fullPath))
                {
                    continue;
                }

                FileInfo file = new(fullPath);
                if (file.Length > MaximumInstructionSizeBytes)
                {
                    continue;
                }

                string content = await File.ReadAllTextAsync(fullPath, cancellationToken);
                instructions.Add(new LocalSetupInstructionPreview(relativePath.Replace('\\', '/'),
                    AgentFor(relativePath), Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(content))),
                    content));
            }

            return new GitRepositorySnapshot(root, remote, instructions.OrderBy(item => item.RelativePath,
                StringComparer.Ordinal).ToArray());
        }

        private static string AgentFor(string path)
        {
            return Path.GetFileName(path).ToUpperInvariant() switch
            {
                "CLAUDE.MD" => "claude",
                "GEMINI.MD" => "gemini",
                _ => "codex"
            };
        }

        private static bool IsInside(string root, string path)
        {
            string prefix = Path.TrimEndingDirectorySeparator(root) + Path.DirectorySeparatorChar;
            return path.StartsWith(prefix, OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);
        }

        private static bool IsEscapingLink(string root, string path)
        {
            FileInfo file = new(path);
            if (file.LinkTarget is null)
            {
                return false;
            }

            FileSystemInfo? target = file.ResolveLinkTarget(true);
            return target is null || !IsInside(root, Path.GetFullPath(target.FullName));
        }
    }
}