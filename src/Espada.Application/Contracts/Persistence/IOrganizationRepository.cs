using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Contracts.Persistence
{
    public interface IOrganizationRepository
    {
        Task AddAsync(
            Organization organization,
            CancellationToken cancellationToken = default);

        Task<Organization?> GetByIdAsync(
            OrganizationId organizationId,
            CancellationToken cancellationToken = default);
    }
}