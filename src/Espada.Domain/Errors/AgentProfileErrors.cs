using Espada.Domain.Rules;

namespace Espada.Domain.Errors
{
    public static class AgentProfileErrors
    {
        public static DomainError NameEmpty { get; } = new("AgentProfile.NameEmpty",
            "Agent profile name cannot be empty.");

        public static DomainError NameTooLong { get; } = new("AgentProfile.NameTooLong",
            "Agent profile name cannot exceed 200 characters.");

        public static DomainError SettingsEmpty { get; } = new("AgentProfile.SettingsEmpty",
            "Agent profile settings cannot be empty.");
    }
}