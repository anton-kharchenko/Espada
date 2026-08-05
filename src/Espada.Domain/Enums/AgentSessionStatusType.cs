using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums
{
    public sealed class AgentSessionStatusType(int id, string name) : Enumeration(id, name)
    {
        public static readonly AgentSessionStatusType Created = new(1, nameof(Created));

        public static readonly AgentSessionStatusType Running = new(2, nameof(Running));

        public static readonly AgentSessionStatusType WaitingForApproval = new(3, nameof(WaitingForApproval));

        public static readonly AgentSessionStatusType Completed = new(4, nameof(Completed));

        public static readonly AgentSessionStatusType Failed = new(5, nameof(Failed));

        public static readonly AgentSessionStatusType Cancelled = new(6, nameof(Cancelled));

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
