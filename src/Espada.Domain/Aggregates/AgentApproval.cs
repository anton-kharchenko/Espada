using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class AgentApproval : AggregateRoot<AgentApprovalId>, IHasConcurrencyVersion
    {
        private AgentApproval()
        {
        }

        private AgentApproval(AgentApprovalId id, AgentSessionId agentSessionId,
            AgentSessionEventId requestEventId, string toolName, string argumentsJson,
            DateTimeOffset requestedAtUtc) : base(id)
        {
            AgentSessionId = agentSessionId;
            RequestEventId = requestEventId;
            ToolName = toolName;
            ArgumentsJson = argumentsJson;
            Status = AgentApprovalStatusType.Pending;
            RequestedAtUtc = requestedAtUtc;
        }

        public AgentSessionId AgentSessionId { get; private set; } = null!;
        public AgentSessionEventId RequestEventId { get; private set; } = null!;
        public string ToolName { get; private set; } = string.Empty;
        public string ArgumentsJson { get; private set; } = "{}";
        public AgentApprovalStatusType Status { get; private set; } = null!;
        public DateTimeOffset RequestedAtUtc { get; private set; }
        public DateTimeOffset? DecidedAtUtc { get; private set; }
        public uint Version { get; private set; }

        public static DomainResult<AgentApproval> Create(AgentApprovalId id, AgentSessionId agentSessionId,
            AgentSessionEventId requestEventId, string? toolName, string? argumentsJson,
            DateTimeOffset requestedAtUtc)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(agentSessionId);
            ArgumentNullException.ThrowIfNull(requestEventId);
            if (string.IsNullOrWhiteSpace(toolName))
            {
                return DomainResult<AgentApproval>.Failure(AgentApprovalErrors.ToolNameEmpty);
            }

            return string.IsNullOrWhiteSpace(argumentsJson)
                ? DomainResult<AgentApproval>.Failure(AgentApprovalErrors.ArgumentsEmpty)
                : DomainResult<AgentApproval>.Success(new AgentApproval(id, agentSessionId, requestEventId,
                    toolName.Trim(), argumentsJson, requestedAtUtc));
        }

        public DomainResult Decide(bool approved, DateTimeOffset decidedAtUtc)
        {
            if (!Status.Equals(AgentApprovalStatusType.Pending))
            {
                return DomainResult.Failure(AgentApprovalErrors.NotPending);
            }

            Status = approved ? AgentApprovalStatusType.Approved : AgentApprovalStatusType.Denied;
            DecidedAtUtc = decidedAtUtc;
            return DomainResult.Success();
        }
    }
}
