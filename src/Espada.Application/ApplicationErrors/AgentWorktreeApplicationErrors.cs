using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors
{
    public static class AgentWorktreeApplicationErrors
    {
        public static readonly DomainError RepositoryUnavailable = new("AgentWorktree.RepositoryUnavailable",
            "The project has no accessible local repository.");
        public static readonly DomainError TargetNotClean = new("AgentWorktree.TargetNotClean",
            "The target repository must be clean before applying a session diff.");

        public static readonly DomainError WorktreeNotManaged = new("AgentWorktree.WorktreeNotManaged",
            "Only an Espada-managed session worktree can be removed.");

        public static DomainError GitFailed(string operation)
        {
            return new DomainError("AgentWorktree.GitFailed", $"Git failed while trying to {operation}.");
        }
    }
}