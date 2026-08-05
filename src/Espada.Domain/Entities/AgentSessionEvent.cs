using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Entities
{
    public sealed class AgentSessionEvent : Entity<AgentSessionEventId>
    {
        private AgentSessionEvent()
        {
        }

        private AgentSessionEvent(AgentSessionEventId id, AgentSessionId agentSessionId, long sequence,
            AgentSessionEventType type, string payloadJson, DateTimeOffset occurredAtUtc) : base(id)
        {
            AgentSessionId = agentSessionId;
            Sequence = sequence;
            Type = type;
            PayloadJson = payloadJson;
            OccurredAtUtc = occurredAtUtc;
        }

        public AgentSessionId AgentSessionId { get; private set; } = null!;
        public long Sequence { get; private set; }
        public AgentSessionEventType Type { get; private set; } = null!;
        public string PayloadJson { get; private set; } = "{}";
        public DateTimeOffset OccurredAtUtc { get; private set; }

        public static DomainResult<AgentSessionEvent> Create(AgentSessionEventId id, AgentSessionId agentSessionId,
            long sequence, AgentSessionEventType type, string? payloadJson, DateTimeOffset occurredAtUtc)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(agentSessionId);
            ArgumentNullException.ThrowIfNull(type);
            if (sequence < 1)
            {
                return DomainResult<AgentSessionEvent>.Failure(AgentSessionEventErrors.SequenceOutOfRange);
            }

            return string.IsNullOrWhiteSpace(payloadJson)
                ? DomainResult<AgentSessionEvent>.Failure(AgentSessionEventErrors.PayloadEmpty)
                : DomainResult<AgentSessionEvent>.Success(new AgentSessionEvent(id, agentSessionId, sequence, type,
                    payloadJson, occurredAtUtc));
        }
    }
}