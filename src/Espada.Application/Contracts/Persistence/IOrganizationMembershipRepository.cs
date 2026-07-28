using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IOrganizationMembershipRepository
    {
        Task AddAsync(
            OrganizationMembership membership,
            CancellationToken cancellationToken = default);

        Task<OrganizationMembership?> GetByIdentityAsync(
            OrganizationId organizationId,
            string issuer,
            string subject,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<OrganizationMembership>> ListByOrganizationIdAsync(
            OrganizationId organizationId,
            CancellationToken cancellationToken = default);
    }
}