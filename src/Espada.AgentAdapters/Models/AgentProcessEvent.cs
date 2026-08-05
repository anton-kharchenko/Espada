using Espada.Domain.Enums;

namespace Espada.AgentAdapters.Models
{
    public sealed record AgentProcessEvent(AgentSessionEventType Type, string PayloadJson);
}