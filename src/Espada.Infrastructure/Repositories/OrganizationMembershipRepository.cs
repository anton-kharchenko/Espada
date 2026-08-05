using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;
using Espada.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Espada.Infrastructure.Repositories
{
    internal sealed class OrganizationMembershipRepository(
        EspadaDbContext dbContext) : IOrganizationMembershipRepository
    {
        public async Task AddAsync(
            OrganizationMembership membership,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(membership);
            await dbContext.OrganizationMemberships.AddAsync(membership, cancellationToken);
        }

        public async Task<OrganizationMembership?> GetByIdentityAsync(
            OrganizationId organizationId,
            string issuer,
            string subject,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(organizationId);
            ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
            ArgumentException.ThrowIfNullOrWhiteSpace(subject);
            return await dbContext.OrganizationMemberships
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    membership => membership.OrganizationId == organizationId
                                  && membership.Issuer == issuer
                                  && membership.Subject == subject,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<OrganizationMembership>> ListByOrganizationIdAsync(
            OrganizationId organizationId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(organizationId);
            return await dbContext.OrganizationMemberships
                .AsNoTracking()
                .Where(membership => membership.OrganizationId == organizationId)
                .OrderBy(membership => membership.Role)
                .ThenBy(membership => membership.Issuer)
                .ThenBy(membership => membership.Subject)
                .ToListAsync(cancellationToken);
        }
    }
}