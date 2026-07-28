using AutoMapper;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.UseCases.Organizations.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Organizations.Commands.CreateOrganization
{
    internal sealed class CreateOrganizationCommandHandler(
        IOrganizationRepository organizationRepository,
        IUnitOfWork unitOfWork,
        IClockService clockService,
        IMapper mapper)
        : ICommandHandler<CreateOrganizationCommand, OrganizationResponse>
    {
        public async Task<DomainResult<OrganizationResponse>> Handle(
            CreateOrganizationCommand request,
            CancellationToken cancellationToken)
        {
            DomainResult<Organization> organizationResult = Organization.Create(
                OrganizationId.Create(Guid.NewGuid()),
                request.Name,
                clockService.UtcNow);
            if (organizationResult.IsFailure)
            {
                return DomainResult.Failure<OrganizationResponse>(organizationResult.Error);
            }

            await organizationRepository.AddAsync(organizationResult.Value, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return DomainResult.Success(mapper.Map<OrganizationResponse>(organizationResult.Value));
        }
    }
}