namespace Espada.Api.Contracts.Responses.AgentSessions
{
    public sealed record AgentOptionsResponse(Guid DeviceId, IReadOnlyList<AgentOptionResponse> Agents);
}