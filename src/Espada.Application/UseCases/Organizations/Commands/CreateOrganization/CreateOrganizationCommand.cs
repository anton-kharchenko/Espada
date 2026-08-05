using Espada.Application.Contracts.Messaging;
using Espada.Application.UseCases.Organizations.Common;

namespace Espada.Application.UseCases.Organizations.Commands.CreateOrganization
{
    public sealed record CreateOrganizationCommand(
        string Name) : ICommand<OrganizationResponse>;
}