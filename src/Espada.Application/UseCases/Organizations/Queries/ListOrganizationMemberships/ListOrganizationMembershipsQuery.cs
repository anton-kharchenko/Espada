using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Organizations.Queries.ListOrganizationMemberships
{
    public sealed record ListOrganizationMembershipsQuery(
        Guid OrganizationId) : IQuery<ListOrganizationMembershipsResponse>;
}