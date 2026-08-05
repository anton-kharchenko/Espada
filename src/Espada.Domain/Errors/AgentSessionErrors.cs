using Espada.Domain.Enums;
using Espada.Domain.Rules;

namespace Espada.Domain.Errors
{
    public static class AgentSessionErrors
    {
        public static DomainError PromptEmpty { get; } = new("AgentSession.PromptEmpty",
            "Agent session prompt cannot be empty.");

        public static DomainError BranchNameEmpty { get; } = new("AgentSession.BranchNameEmpty",
            "Agent session branch name cannot be empty.");

        public static DomainError WorktreePathEmpty { get; } = new("AgentSession.WorktreePathEmpty",
            "Agent session worktree path cannot be empty.");

        public static DomainError InvalidTransition(AgentSessionStatusType current, AgentSessionStatusType requested)
        {
            return new DomainError("AgentSession.InvalidTransition",
                $"Agent session cannot transition from {current.Name} to {requested.Name}.");
        }
    }
}