using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.AgentSessions.Commands.StartAgentSessions
{
    public sealed record StartAgentSessionsCommand(Guid WorkspaceId, Guid ProjectId, Guid DeviceId, string Prompt,
        IReadOnlyList<Guid> AgentProfileIds) : ICommand<StartAgentSessionsResponse>;
}