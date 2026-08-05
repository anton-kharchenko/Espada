using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class OrganizationRepository(
        EspadaDbContext dbContext) : IOrganizationRepository
    {
        public async Task AddAsync(
            Organization organization,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(organization);
            await dbContext.Organizations.AddAsync(organization, cancellationToken);
        }

        public async Task<Organization?> GetByIdAsync(
            OrganizationId organizationId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(organizationId);
            return await dbContext.Organizations.FindAsync([organizationId], cancellationToken);
        }
    }
}