using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors
{
    public static class OrganizationApplicationErrors
    {
        public static readonly DomainError InvalidId = new(
            "Organization.Id.Invalid",
            "Organization ID cannot be empty.");

        public static DomainError NotFound(Guid organizationId)
        {
            return new DomainError(
                "Organization.NotFound",
                $"Organization with ID '{organizationId:D}' was not found.");
        }

        public static DomainError UnsupportedRoleType(int roleTypeId)
        {
            return new DomainError(
                "OrganizationMembership.RoleType.Unsupported",
                $"Organization membership role type with ID '{roleTypeId}' is not supported.");
        }

        public static DomainError DuplicateMember(string issuer, string subject)
        {
            return new DomainError(
                "OrganizationMembership.Identity.Duplicate",
                $"Identity '{issuer}|{subject}' is already an organization member.");
        }
    }
}