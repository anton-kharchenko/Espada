namespace Espada.Application.Models.Agents
{
    public sealed record AgentWorktree(string RepositoryRoot, string BranchName, string WorktreePath);
}