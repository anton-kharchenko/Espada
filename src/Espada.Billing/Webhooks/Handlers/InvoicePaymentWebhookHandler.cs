using Espada.Billing.Constants;
using Espada.Billing.Contracts;
using Espada.Billing.Models;
using Stripe;

namespace Espada.Billing.Webhooks.Handlers
{
    internal sealed class InvoicePaymentWebhookHandler(IBillingStoreService storeService) : IStripeWebhookHandler
    {
        public bool CanHandle(string eventType)
        {
            return eventType is EventTypes.InvoicePaymentSucceeded or EventTypes.InvoicePaymentFailed;
        }

        public async Task HandleAsync(Event stripeEvent, CancellationToken cancellationToken)
        {
            if (stripeEvent.Data.Object is not Invoice invoice || string.IsNullOrWhiteSpace(invoice.CustomerId))
            {
                throw new InvalidOperationException("Invoice event is missing its customer.");
            }

            bool failed = stripeEvent.Type == EventTypes.InvoicePaymentFailed;
            await storeService.ApplyCustomerUpdateAsync(
                new BillingCustomerUpdate(null, invoice.CustomerId, null, null,
                    failed ? BillingSubscriptionStatusConstants.PastDue : BillingSubscriptionStatusConstants.Active,
                    failed ? stripeEvent.Created : null, stripeEvent.Created), cancellationToken);
        }
    }
}