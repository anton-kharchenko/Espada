using Espada.Domain.Rules;

namespace Espada.Domain.Errors
{
    public static class OrganizationMembershipErrors
    {
        public static DomainError IssuerEmpty { get; } = new("OrganizationMembership.IssuerEmpty",
            "Organization membership issuer cannot be empty.");

        public static DomainError IssuerTooLong { get; } = new("OrganizationMembership.IssuerTooLong",
            "Organization membership issuer cannot exceed 500 characters.");

        public static DomainError SubjectEmpty { get; } = new("OrganizationMembership.SubjectEmpty",
            "Organization membership subject cannot be empty.");

        public static DomainError SubjectTooLong { get; } = new("OrganizationMembership.SubjectTooLong",
            "Organization membership subject cannot exceed 200 characters.");
    }
}