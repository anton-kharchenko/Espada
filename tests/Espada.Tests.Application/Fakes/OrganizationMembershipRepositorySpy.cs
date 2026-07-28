using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class OrganizationMembershipRepositorySpy
        : IOrganizationMembershipRepository
    {
        public OrganizationMembership? MembershipToReturn { get; set; }

        public int GetByIdentityCallCount { get; private set; }

        public Task AddAsync(
            OrganizationMembership membership,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<OrganizationMembership?> GetByIdentityAsync(
            OrganizationId organizationId,
            string issuer,
            string subject,
            CancellationToken cancellationToken = default)
        {
            GetByIdentityCallCount++;
            return Task.FromResult(MembershipToReturn);
        }

        public Task<IReadOnlyList<OrganizationMembership>>
            ListByOrganizationIdAsync(
                OrganizationId organizationId,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<OrganizationMembership>>([]);
        }
    }
}