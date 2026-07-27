using Espada.Billing.Constants;
using Espada.Billing.Contracts;
using Espada.Billing.Models;
using Microsoft.Extensions.Options;
using Stripe;

namespace Espada.Billing.Webhooks;

internal sealed class StripeWebhookIngestor(IBillingStoreService storeService, IOptions<BillingOptions> options) : IStripeWebhookIngestor
{
    public Task<bool> AcceptAsync(string payload, string signature, CancellationToken cancellationToken = default)
    {
        Event stripeEvent = EventUtility.ConstructEvent(payload, signature, options.Value.StripeWebhookSecret, tolerance: 300, throwOnApiVersionMismatch: true);
        PaymentEventEnvelope envelope = new(stripeEvent.Id, stripeEvent.Type, stripeEvent.ApiVersion ?? BillingConstants.RequiredStripeApiVersion, payload, stripeEvent.Created, DateTimeOffset.UtcNow);
        return storeService.AddPaymentEventAsync(envelope, cancellationToken);
    }
}