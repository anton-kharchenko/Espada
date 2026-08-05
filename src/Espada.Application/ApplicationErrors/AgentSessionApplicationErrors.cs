using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors
{
    public static class AgentSessionApplicationErrors
    {
        public static readonly DomainError InvalidRequest = new("AgentSession.InvalidRequest",
            "Workspace, project, device, prompt, and at least one agent profile are required.");

        public static DomainError ProfileNotFound(Guid profileId)
        {
            return new DomainError("AgentSession.ProfileNotFound",
                $"Agent profile '{profileId}' was not found in the workspace.");
        }

        public static DomainError InstallationUnavailable(int vendorId)
        {
            return new DomainError("AgentSession.InstallationUnavailable",
                $"Agent vendor '{vendorId}' is not installed and authenticated on this device.");
        }
    }
}