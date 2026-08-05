namespace Espada.Api.Contracts.Requests.AgentSessions
{
    public sealed record StartAgentSessionsRequest(Guid ProjectId, Guid DeviceId, string Prompt,
        IReadOnlyList<Guid> AgentProfileIds);
}