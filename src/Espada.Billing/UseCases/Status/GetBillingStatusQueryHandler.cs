using Espada.Application.Contracts.Messaging;
using Espada.Application.Contracts.Time;
using Espada.Billing.ApplicationErrors;
using Espada.Billing.Contracts;
using Espada.Billing.Models;
using Espada.Billing.Options;
using Espada.Domain.Rules;
using Microsoft.Extensions.Options;

namespace Espada.Billing.UseCases.Status
{
    internal sealed class GetBillingStatusQueryHandler(
        IBillingStoreService storeService,
        IOptions<BillingOptions> options,
        IClockService clock) : IQueryHandler<GetBillingStatusQuery, BillingStatusSnapshot>
    {
        public async Task<DomainResult<BillingStatusSnapshot>> Handle(GetBillingStatusQuery request,
            CancellationToken cancellationToken)
        {
            if (!options.Value.Enabled)
            {
                return DomainResult.Failure<BillingStatusSnapshot>(BillingApplicationErrors.Unavailable);
            }

            BillingCustomerSnapshot? customer =
                await storeService.GetCustomerByWorkspaceAsync(request.WorkspaceId, cancellationToken);
            return customer is null
                ? DomainResult.Failure<BillingStatusSnapshot>(BillingApplicationErrors.CustomerNotFound)
                : DomainResult.Success(new BillingStatusSnapshot(customer.Plan, customer.SubscriptionStatus,
                    customer.GetAccessState(clock.UtcNow), customer.PaymentFailedAtUtc, true));
        }
    }
}