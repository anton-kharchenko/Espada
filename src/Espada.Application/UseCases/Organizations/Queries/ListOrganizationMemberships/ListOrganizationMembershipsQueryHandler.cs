using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.UseCases.Organizations.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Organizations.Queries.ListOrganizationMemberships
{
    internal sealed class ListOrganizationMembershipsQueryHandler(
        IOrganizationRepository organizationRepository,
        IOrganizationMembershipRepository membershipRepository,
        IMapper mapper)
        : IQueryHandler<ListOrganizationMembershipsQuery, ListOrganizationMembershipsResponse>
    {
        public async Task<DomainResult<ListOrganizationMembershipsResponse>> Handle(
            ListOrganizationMembershipsQuery request,
            CancellationToken cancellationToken)
        {
            if (request.OrganizationId == Guid.Empty)
            {
                return DomainResult.Failure<ListOrganizationMembershipsResponse>(
                    OrganizationApplicationErrors.InvalidId);
            }

            OrganizationId organizationId = OrganizationId.Create(request.OrganizationId);
            Organization? organization = await organizationRepository.GetByIdAsync(
                organizationId,
                cancellationToken);
            if (organization is null)
            {
                return DomainResult.Failure<ListOrganizationMembershipsResponse>(
                    OrganizationApplicationErrors.NotFound(request.OrganizationId));
            }

            IReadOnlyList<OrganizationMembership> memberships =
                await membershipRepository.ListByOrganizationIdAsync(
                    organizationId,
                    cancellationToken);
            OrganizationMembershipResponse[] items =
                mapper.Map<OrganizationMembershipResponse[]>(memberships);

            return DomainResult.Success(new ListOrganizationMembershipsResponse(items));
        }
    }
}