using Espada.Application.Contracts.Messaging;
using Espada.Billing.Enums;
using Espada.Billing.Models;

namespace Espada.Billing.UseCases.Checkout
{
    public sealed record CreateCheckoutCommand(Guid WorkspaceId, CloudBillingPlanType Plan, string IdempotencyKey)
        : ICommand<HostedBillingSession>;
}