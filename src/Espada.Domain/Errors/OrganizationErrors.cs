using Espada.Domain.Rules;

namespace Espada.Domain.Errors
{
    public static class OrganizationErrors
    {
        public static DomainError NameEmpty { get; } =
            new("Organization.NameEmpty", "Organization name cannot be empty.");

        public static DomainError NameTooLong { get; } = new("Organization.NameTooLong",
            "Organization name cannot exceed 200 characters.");
    }
}