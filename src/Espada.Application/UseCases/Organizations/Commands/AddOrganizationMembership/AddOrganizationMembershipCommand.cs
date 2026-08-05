using Espada.Application.Contracts.Messaging;
using Espada.Application.UseCases.Organizations.Common;

namespace Espada.Application.UseCases.Organizations.Commands.AddOrganizationMembership
{
    public sealed record AddOrganizationMembershipCommand(
        Guid OrganizationId,
        string Issuer,
        string Subject,
        int RoleTypeId) : ICommand<OrganizationMembershipResponse>;
}