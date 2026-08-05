using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums
{
    public sealed class AgentApprovalStatusType(int id, string name) : Enumeration(id, name)
    {
        public static readonly AgentApprovalStatusType Pending = new(1, nameof(Pending));

        public static readonly AgentApprovalStatusType Approved = new(2, nameof(Approved));

        public static readonly AgentApprovalStatusType Denied = new(3, nameof(Denied));

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