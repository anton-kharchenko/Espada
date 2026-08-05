using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Agents;
using Espada.Application.Models.Agents;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.AgentAdapters.Git
{
    public sealed class AgentWorktreeService : IAgentWorktreeService
    {
        public async Task<DomainResult<AgentWorktree>> PrepareAsync(Project project, AgentSessionId sessionId,
            AgentVendorType vendor, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(project);
            ArgumentNullException.ThrowIfNull(sessionId);
            ArgumentNullException.ThrowIfNull(vendor);
            string? repositoryRoot = project.LocalAliases.FirstOrDefault(Directory.Exists);
            if (repositoryRoot is null)
            {
                return DomainResult.Failure<AgentWorktree>(AgentWorktreeApplicationErrors.RepositoryUnavailable);
            }

            repositoryRoot = Path.GetFullPath(repositoryRoot);
            string dataRoot = Environment.GetEnvironmentVariable("ESPADA_DATA_ROOT")
                ?? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Espada");
            string worktreePath = Path.Join(dataRoot, "worktrees", sessionId.Value.ToString("N"));
            string branchName = $"espada/{vendor.Name.ToLowerInvariant()}/{sessionId.Value:N}";
            if (!Directory.Exists(worktreePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(worktreePath)!);
                GitCommandResult result = await GitCommand.RunAsync(repositoryRoot,
                    ["worktree", "add", "-b", branchName, worktreePath, "HEAD"], cancellationToken: cancellationToken);
                if (!result.IsSuccess)
                {
                    return DomainResult.Failure<AgentWorktree>(
                        AgentWorktreeApplicationErrors.GitFailed("create the isolated worktree"));
                }
            }

            return DomainResult.Success(new AgentWorktree(repositoryRoot, branchName, worktreePath));
        }

        public async Task<DomainResult> ApplyAsync(Project project, AgentWorktree worktree,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(project);
            ArgumentNullException.ThrowIfNull(worktree);
            GitCommandResult status = await GitCommand.RunAsync(worktree.RepositoryRoot,
                ["status", "--porcelain"], cancellationToken: cancellationToken);
            if (!status.IsSuccess)
            {
                return DomainResult.Failure(AgentWorktreeApplicationErrors.GitFailed("inspect the target repository"));
            }

            if (!string.IsNullOrWhiteSpace(status.StandardOutput))
            {
                return DomainResult.Failure(AgentWorktreeApplicationErrors.TargetNotClean);
            }

            GitCommandResult diff = await GitCommand.RunAsync(worktree.WorktreePath,
                ["diff", "--binary", "HEAD"], cancellationToken: cancellationToken);
            if (!diff.IsSuccess)
            {
                return DomainResult.Failure(AgentWorktreeApplicationErrors.GitFailed("create the session diff"));
            }

            if (string.IsNullOrWhiteSpace(diff.StandardOutput))
            {
                return DomainResult.Success();
            }

            GitCommandResult check = await GitCommand.RunAsync(worktree.RepositoryRoot,
                ["apply", "--check", "-"], diff.StandardOutput, cancellationToken);
            if (!check.IsSuccess)
            {
                return DomainResult.Failure(AgentWorktreeApplicationErrors.GitFailed("validate the session diff"));
            }

            GitCommandResult apply = await GitCommand.RunAsync(worktree.RepositoryRoot,
                ["apply", "-"], diff.StandardOutput, cancellationToken);
            return apply.IsSuccess
                ? DomainResult.Success()
                : DomainResult.Failure(AgentWorktreeApplicationErrors.GitFailed("apply the session diff"));
        }
        public async Task<DomainResult> RemoveAsync(Project project, AgentWorktree worktree,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(project);
            ArgumentNullException.ThrowIfNull(worktree);
            string dataRoot = Environment.GetEnvironmentVariable("ESPADA_DATA_ROOT")
                              ?? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                  "Espada");
            string managedRoot = Path.TrimEndingDirectorySeparator(
                                     Path.GetFullPath(Path.Join(dataRoot, "worktrees")))
                                 + Path.DirectorySeparatorChar;
            string worktreePath = Path.GetFullPath(worktree.WorktreePath);
            StringComparison comparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
            if (!worktreePath.StartsWith(managedRoot, comparison)
                || !worktree.BranchName.StartsWith("espada/", StringComparison.Ordinal))
            {
                return DomainResult.Failure(AgentWorktreeApplicationErrors.WorktreeNotManaged);
            }

            if (Directory.Exists(worktreePath))
            {
                GitCommandResult remove = await GitCommand.RunAsync(worktree.RepositoryRoot,
                    ["worktree", "remove", "--force", worktreePath], cancellationToken: cancellationToken);
                if (!remove.IsSuccess)
                {
                    return DomainResult.Failure(
                        AgentWorktreeApplicationErrors.GitFailed("remove the session worktree"));
                }
            }

            GitCommandResult branch = await GitCommand.RunAsync(worktree.RepositoryRoot,
                ["branch", "--list", worktree.BranchName], cancellationToken: cancellationToken);
            if (!branch.IsSuccess)
            {
                return DomainResult.Failure(AgentWorktreeApplicationErrors.GitFailed("inspect the session branch"));
            }

            if (!string.IsNullOrWhiteSpace(branch.StandardOutput))
            {
                GitCommandResult delete = await GitCommand.RunAsync(worktree.RepositoryRoot,
                    ["branch", "-D", worktree.BranchName], cancellationToken: cancellationToken);
                if (!delete.IsSuccess)
                {
                    return DomainResult.Failure(
                        AgentWorktreeApplicationErrors.GitFailed("delete the session branch"));
                }
            }

            return DomainResult.Success();
        }

    }
}