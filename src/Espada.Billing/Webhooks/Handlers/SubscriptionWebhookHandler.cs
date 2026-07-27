using Espada.Billing.Constants;
using Espada.Billing.Contracts;
using Espada.Billing.Enums;
using Espada.Billing.Models;
using Stripe;

namespace Espada.Billing.Webhooks.Handlers;

internal sealed class SubscriptionWebhookHandler(IBillingStoreService storeService) : IStripeWebhookHandler
{
    public bool CanHandle(string eventType) => eventType is EventTypes.CustomerSubscriptionCreated or EventTypes.CustomerSubscriptionUpdated or EventTypes.CustomerSubscriptionDeleted;

    public async Task HandleAsync(Event stripeEvent, CancellationToken cancellationToken)
    {
        if (stripeEvent.Data.Object is not Subscription subscription || string.IsNullOrWhiteSpace(subscription.CustomerId))
        {
            throw new InvalidOperationException("Subscription event is missing its customer.");
        }

        Guid? workspaceId = subscription.Metadata.TryGetValue(StripeMetadataKeyContants.WorkspaceId, out string? workspaceValue) && Guid.TryParse(workspaceValue, out Guid parsedWorkspaceId) ? parsedWorkspaceId : null;
        CloudBillingPlanType? plan = subscription.Metadata.TryGetValue(StripeMetadataKeyContants.Plan, out string? planValue) && Enum.TryParse(planValue, ignoreCase: true, out CloudBillingPlanType parsedPlan) ? parsedPlan : null;
        await storeService.ApplyCustomerUpdateAsync(new BillingCustomerUpdate(workspaceId, subscription.CustomerId, subscription.Id, plan, subscription.Status, null, stripeEvent.Created), cancellationToken);
    }
}