using Espada.Domain.Rules;

namespace Espada.Domain.Errors
{
    public static class AgentApprovalErrors
    {
        public static DomainError ToolNameEmpty { get; } = new("AgentApproval.ToolNameEmpty",
            "Approval tool name cannot be empty.");

        public static DomainError ArgumentsEmpty { get; } = new("AgentApproval.ArgumentsEmpty",
            "Approval arguments cannot be empty.");

        public static DomainError NotPending { get; } = new("AgentApproval.NotPending",
            "Agent approval has already been resolved.");
    }
}