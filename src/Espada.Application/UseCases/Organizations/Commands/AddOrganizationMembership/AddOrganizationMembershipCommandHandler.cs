using AutoMapper;
using Espada.Application.ApplicationErrors;
using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Persistence;
using Espada.Application.Contracts.Time;
using Espada.Application.UseCases.Organizations.Common;
using Espada.Domain.Aggregates;
using Espada.Domain.Enums;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Application.UseCases.Organizations.Commands.AddOrganizationMembership
{
    internal sealed class AddOrganizationMembershipCommandHandler(
        IOrganizationRepository organizationRepository,
        IOrganizationMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork,
        IClockService clockService,
        IMapper mapper)
        : ICommandHandler<AddOrganizationMembershipCommand, OrganizationMembershipResponse>
    {
        public async Task<DomainResult<OrganizationMembershipResponse>> Handle(
            AddOrganizationMembershipCommand request,
            CancellationToken cancellationToken)
        {
            if (request.OrganizationId == Guid.Empty)
            {
                return DomainResult.Failure<OrganizationMembershipResponse>(
                    OrganizationApplicationErrors.InvalidId);
            }

            OrganizationId organizationId = OrganizationId.Create(request.OrganizationId);
            Organization? organization = await organizationRepository.GetByIdAsync(
                organizationId,
                cancellationToken);
            if (organization is null)
            {
                return DomainResult.Failure<OrganizationMembershipResponse>(
                    OrganizationApplicationErrors.NotFound(request.OrganizationId));
            }

            OrganizationMembershipRoleType? roleType = Enumeration
                .GetAll<OrganizationMembershipRoleType>()
                .SingleOrDefault(value => value.Id == request.RoleTypeId);
            if (roleType is null)
            {
                return DomainResult.Failure<OrganizationMembershipResponse>(
                    OrganizationApplicationErrors.UnsupportedRoleType(request.RoleTypeId));
            }

            DomainResult<OrganizationMembership> membershipResult = organization.CreateMembership(
                OrganizationMembershipId.Create(Guid.NewGuid()),
                request.Issuer,
                request.Subject,
                roleType,
                clockService.UtcNow);
            if (membershipResult.IsFailure)
            {
                return DomainResult.Failure<OrganizationMembershipResponse>(membershipResult.Error);
            }

            OrganizationMembership membership = membershipResult.Value;
            OrganizationMembership? existing = await membershipRepository.GetByIdentityAsync(
                organizationId,
                membership.Issuer,
                membership.Subject,
                cancellationToken);
            if (existing is not null)
            {
                return DomainResult.Failure<OrganizationMembershipResponse>(
                    OrganizationApplicationErrors.DuplicateMember(
                        membership.Issuer,
                        membership.Subject));
            }

            await membershipRepository.AddAsync(membership, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return DomainResult.Success(
                mapper.Map<OrganizationMembershipResponse>(membership));
        }
    }
}