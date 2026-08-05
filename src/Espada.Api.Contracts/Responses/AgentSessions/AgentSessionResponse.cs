namespace Espada.Api.Contracts.Responses.AgentSessions
{
    public sealed record AgentSessionResponse(Guid SessionId, Guid ProjectId, Guid AgentProfileId, string Prompt,
        string BranchName, string Status, DateTimeOffset CreatedAtUtc, DateTimeOffset? FinishedAtUtc);
}