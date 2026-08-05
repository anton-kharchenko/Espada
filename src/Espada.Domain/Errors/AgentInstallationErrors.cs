using Espada.Domain.Rules;

namespace Espada.Domain.Errors
{
    public static class AgentInstallationErrors
    {
        public static DomainError ExecutablePathEmpty { get; } = new("AgentInstallation.ExecutablePathEmpty",
            "Agent executable path cannot be empty.");

        public static DomainError ExecutablePathTooLong { get; } = new("AgentInstallation.ExecutablePathTooLong",
            "Agent executable path cannot exceed 2048 characters.");
    }
}
