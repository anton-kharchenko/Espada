using Espada.Application.Contracts.Messaging;
using Espada.Application.UseCases.Organizations.Common;

namespace Espada.Application.UseCases.Organizations.Queries.GetOrganization
{
    public sealed record GetOrganizationQuery(
        Guid OrganizationId) : IQuery<OrganizationResponse>;
}