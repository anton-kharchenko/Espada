using Espada.Application.Contracts.Messaging;
using Espada.Billing.ApplicationErrors;
using Espada.Billing.Contracts;
using Espada.Billing.Models;
using Espada.Domain.Rules;
using Microsoft.Extensions.Options;

namespace Espada.Billing.UseCases.Portal;

internal sealed class CreateCustomerPortalCommandHandler(IEnumerable<IStripeBillingProvider> billingProviders, IBillingStoreService storeService, IOptions<BillingOptions> options) : ICommandHandler<CreateCustomerPortalCommand, HostedBillingSession>
{
    public async Task<DomainResult<HostedBillingSession>> Handle(CreateCustomerPortalCommand request, CancellationToken cancellationToken)
    {
        IStripeBillingProvider? provider = billingProviders.SingleOrDefault();
        if (!options.Value.Enabled || provider is null)
        {
            return DomainResult.Failure<HostedBillingSession>(BillingApplicationErrors.Unavailable);
        }

        BillingCustomerSnapshot? customer = await storeService.GetCustomerByWorkspaceAsync(request.WorkspaceId, cancellationToken);
        if (customer is null)
        {
            return DomainResult.Failure<HostedBillingSession>(BillingApplicationErrors.CustomerNotFound);
        }

        HostedBillingSession session = await provider.CreateCustomerPortalAsync(customer.ProviderCustomerId, request.IdempotencyKey, cancellationToken);
        return DomainResult.Success(session);
    }
}