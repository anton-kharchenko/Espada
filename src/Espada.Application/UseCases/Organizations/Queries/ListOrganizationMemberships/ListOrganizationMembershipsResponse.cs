using Espada.Application.UseCases.Organizations.Common;

namespace Espada.Application.UseCases.Organizations.Queries.ListOrganizationMemberships
{
    public sealed record ListOrganizationMembershipsResponse(
        IReadOnlyList<OrganizationMembershipResponse> Items);
}