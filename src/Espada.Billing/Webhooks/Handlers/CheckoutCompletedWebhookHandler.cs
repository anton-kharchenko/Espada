using Espada.Billing.Contracts;
using Espada.Billing.Enums;
using Espada.Billing.Models;
using Stripe;
using Stripe.Checkout;

namespace Espada.Billing.Webhooks.Handlers;

internal sealed class CheckoutCompletedWebhookHandler(IBillingStoreService storeService) : IStripeWebhookHandler
{
    public bool CanHandle(string eventType) => eventType == "checkout.session.completed";

    public async Task HandleAsync(Event stripeEvent, CancellationToken cancellationToken)
    {
        if (stripeEvent.Data.Object is not Session session || !Guid.TryParse(session.ClientReferenceId, out Guid workspaceId) || string.IsNullOrWhiteSpace(session.CustomerId))
        {
            throw new InvalidOperationException("Checkout session is missing workspace or customer metadata.");
        }

        CloudBillingPlanType plan = ParsePlan(session.Metadata);
        await storeService.ApplyCustomerUpdateAsync(new BillingCustomerUpdate(workspaceId, session.CustomerId, session.SubscriptionId, plan, "active", null, stripeEvent.Created), cancellationToken);
    }

    private static CloudBillingPlanType ParsePlan(IReadOnlyDictionary<string, string> metadata) =>
        metadata.TryGetValue("plan", out string? plan) && Enum.TryParse(plan, ignoreCase: true, out CloudBillingPlanType parsed) ? parsed : throw new InvalidOperationException("Checkout session is missing a valid plan.");
}