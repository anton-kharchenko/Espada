using Espada.Application.Contracts.Messaging;
using Espada.Billing.ApplicationErrors;
using Espada.Billing.Contracts;
using Espada.Billing.Models;
using Espada.Billing.Options;
using Espada.Domain.Rules;
using Microsoft.Extensions.Options;

namespace Espada.Billing.UseCases.Checkout
{
    internal sealed class CreateCheckoutCommandHandler(
        IEnumerable<IStripeBillingProvider> billingProviders,
        IBillingStoreService storeService,
        IOptions<BillingOptions> options) : ICommandHandler<CreateCheckoutCommand, HostedBillingSession>
    {
        public async Task<DomainResult<HostedBillingSession>> Handle(CreateCheckoutCommand request,
            CancellationToken cancellationToken)
        {
            IStripeBillingProvider? provider = billingProviders.SingleOrDefault();
            if (!options.Value.Enabled || provider is null)
            {
                return DomainResult.Failure<HostedBillingSession>(BillingApplicationErrors.Unavailable);
            }

            BillingCustomerSnapshot? customer =
                await storeService.GetCustomerByWorkspaceAsync(request.WorkspaceId, cancellationToken);
            HostedBillingSession session = await provider.CreateCheckoutAsync(request.WorkspaceId,
                customer?.ProviderCustomerId, request.Plan, request.IdempotencyKey, cancellationToken);

            return DomainResult.Success(session);
        }
    }
}