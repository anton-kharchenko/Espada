namespace Espada.Api.Contracts.Responses.AgentSessions
{
    public sealed record AgentApprovalResponse(Guid ApprovalId, Guid SessionId, string ToolName, string ArgumentsJson,
        string Status, DateTimeOffset RequestedAtUtc, DateTimeOffset? DecidedAtUtc);
}