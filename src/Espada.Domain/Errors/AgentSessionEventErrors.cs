using Espada.Domain.Rules;

namespace Espada.Domain.Errors
{
    public static class AgentSessionEventErrors
    {
        public static DomainError SequenceOutOfRange { get; } = new("AgentSessionEvent.SequenceOutOfRange",
            "Agent session event sequence must be positive.");

        public static DomainError PayloadEmpty { get; } = new("AgentSessionEvent.PayloadEmpty",
            "Agent session event payload cannot be empty.");
    }
}
