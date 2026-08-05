namespace Espada.Application.UseCases.AgentSessions.Commands.StartAgentSessions
{
    public sealed record StartAgentSessionsResponse(IReadOnlyList<Guid> SessionIds);
}