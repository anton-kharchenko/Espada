using Espada.Domain.SeedWork;

namespace Espada.Domain.Enums;

public sealed class OrganizationMembershipRoleType(int id, string name) : Enumeration(id, name)
{
    public static readonly OrganizationMembershipRoleType Owner = new(1, nameof(Owner));
    
    public static readonly OrganizationMembershipRoleType Member = new(2, nameof(Member));
}
