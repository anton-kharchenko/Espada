namespace Espada.Api.Contracts.Responses.AgentSessions
{
    public sealed record AgentSessionEventResponse(Guid EventId, long Sequence, string Type, string PayloadJson,
        DateTimeOffset OccurredAtUtc);
}