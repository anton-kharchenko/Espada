using Espada.Application.Contracts.Persistence;
using Espada.Domain.Aggregates;
using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class OrganizationRepositorySpy : IOrganizationRepository
    {
        public Organization? OrganizationToReturn { get; set; }

        public Task AddAsync(Organization organization, CancellationToken cancellationToken = default)
        {
            OrganizationToReturn = organization;
            return Task.CompletedTask;
        }

        public Task<Organization?> GetByIdAsync(OrganizationId organizationId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(OrganizationToReturn);
        }
    }
}