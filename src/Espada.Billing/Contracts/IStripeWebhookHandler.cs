using Stripe;

namespace Espada.Billing.Contracts
{
    internal interface IStripeWebhookHandler
    {
        bool CanHandle(string eventType);

        Task HandleAsync(
            Event stripeEvent,
            CancellationToken cancellationToken);
    }
}