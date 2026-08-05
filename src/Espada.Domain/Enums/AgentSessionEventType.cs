using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums
{
    public sealed class AgentSessionEventType(int id, string name) : Enumeration(id, name)
    {
        public static readonly AgentSessionEventType AssistantOutput = new(1, nameof(AssistantOutput));

        public static readonly AgentSessionEventType ToolRequest = new(2, nameof(ToolRequest));

        public static readonly AgentSessionEventType ToolResult = new(3, nameof(ToolResult));

        public static readonly AgentSessionEventType ApprovalRequest = new(4, nameof(ApprovalRequest));

        public static readonly AgentSessionEventType Status = new(5, nameof(Status));

        public static readonly AgentSessionEventType Usage = new(6, nameof(Usage));

        public static readonly AgentSessionEventType Error = new(7, nameof(Error));

        public static readonly AgentSessionEventType DiffUpdate = new(8, nameof(DiffUpdate));

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}