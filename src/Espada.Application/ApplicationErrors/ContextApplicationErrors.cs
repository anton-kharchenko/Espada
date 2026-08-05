using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors
{
    public static class ContextApplicationErrors
    {
        public static readonly DomainError AgentUnsupported = new(
            "Context.Agent.Unsupported",
            "Agent must be one of: codex, claude, gemini, generic.");

        public static readonly DomainError ProjectRequired = new(
            "Context.Project.Required",
            "Project ID is required when task, path, or branch context is supplied.");

        public static readonly DomainError RepositoryRelativePathInvalid = new(
            "Context.RepositoryRelativePath.Invalid",
            "Repository-relative path cannot be rooted or contain traversal segments.");

        public static readonly DomainError TokenBudgetInvalid = new(
            "Context.TokenBudget.Invalid",
            "Token budget must be a positive UTF-8 byte budget.");

        public static readonly DomainError BudgetTooSmall = new(
            "Context.Budget.TooSmall",
            "Context budget is too small to include all hard policies.");
    }
}