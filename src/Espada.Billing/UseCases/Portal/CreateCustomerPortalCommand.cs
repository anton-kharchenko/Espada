using Espada.Application.Contracts.Messaging;
using Espada.Billing.Models;

namespace Espada.Billing.UseCases.Portal
{
    public sealed record CreateCustomerPortalCommand(Guid WorkspaceId, string IdempotencyKey)
        : ICommand<HostedBillingSession>;
}