using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums
{
    public sealed class WorkspaceMembershipRoleType(int id, string name) : Enumeration(id, name)
    {
        public static readonly WorkspaceMembershipRoleType Owner = new(1, nameof(Owner));

        public static readonly WorkspaceMembershipRoleType Member = new(2, nameof(Member));
    }
}