using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors;

public static class AccessPolicyErrors
{
    public static readonly DomainError Unauthorized = new(
        "Access.Unauthorized",
        "An authenticated Espada principal is required.");

    public static DomainError MissingScope(string scope) => new(
        "Access.Forbidden.MissingScope",
        $"The '{scope}' scope is required.");

    public static readonly DomainError WorkspaceMismatch = new(
        "Access.Forbidden.WorkspaceMismatch",
        "The requested workspace does not match the authorized workspace.");

    public static readonly DomainError WorkspaceMembershipRequired = new(
        "Access.Forbidden.WorkspaceMembershipRequired",
        "Active workspace membership is required.");

    public static readonly DomainError OrganizationMembershipRequired = new(
        "Access.Forbidden.OrganizationMembershipRequired",
        "Active organization membership is required.");
}
