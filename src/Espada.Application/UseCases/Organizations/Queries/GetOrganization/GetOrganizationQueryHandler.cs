using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.UseCases.Organizations.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Organizations.Queries.GetOrganization
{
    internal sealed class GetOrganizationQueryHandler(
        IOrganizationRepository organizationRepository,
        IMapper mapper)
        : IQueryHandler<GetOrganizationQuery, OrganizationResponse>
    {
        public async Task<DomainResult<OrganizationResponse>> Handle(
            GetOrganizationQuery request,
            CancellationToken cancellationToken)
        {
            if (request.OrganizationId == Guid.Empty)
            {
                return DomainResult.Failure<OrganizationResponse>(
                    OrganizationApplicationErrors.InvalidId);
            }

            Organization? organization = await organizationRepository.GetByIdAsync(
                OrganizationId.Create(request.OrganizationId),
                cancellationToken);
            if (organization is null)
            {
                return DomainResult.Failure<OrganizationResponse>(
                    OrganizationApplicationErrors.NotFound(request.OrganizationId));
            }

            return DomainResult.Success(mapper.Map<OrganizationResponse>(organization));
        }
    }
}