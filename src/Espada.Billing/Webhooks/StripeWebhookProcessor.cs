using Espada.Application.Contracts.Time;
using Espada.Billing.Constants;
using Espada.Billing.Contracts;
using Espada.Billing.Models;
using Espada.Billing.Services;
using Stripe;

namespace Espada.Billing.Webhooks;

internal sealed class StripeWebhookProcessor(IBillingStoreService storeService, IEnumerable<IStripeWebhookHandler> handlers, IClockService clock) : IStripeWebhookProcessor
{
    public async Task<bool> ProcessNextAsync(string workerId, CancellationToken cancellationToken = default)
    {
        ClaimedPaymentEvent? claimed = await storeService.ClaimPaymentEventAsync(workerId, BillingProcessingPolicy.LeaseDuration, cancellationToken);
        if (claimed is null)
        {
            return false;
        }

        try
        {
            Event stripeEvent = EventUtility.ParseEvent(claimed.PayloadJson, throwOnApiVersionMismatch: true);
            IStripeWebhookHandler? handler = handlers.SingleOrDefault(candidate => candidate.CanHandle(claimed.EventType));
            if (handler is not null)
            {
                await handler.HandleAsync(stripeEvent, cancellationToken);
            }

            await storeService.MarkPaymentEventProcessedAsync(claimed.ProviderEventId, workerId, cancellationToken);
        }
        catch (Exception exception)
        {
            bool retryable = exception is StripeException or HttpRequestException or IOException or TimeoutException;
            int retryIndex = claimed.Attempt - 1;
            retryable &= retryIndex < BillingProcessingPolicy.WebhookRetryDelays.Count;
            DateTimeOffset availableAtUtc = retryable ? clock.UtcNow + BillingProcessingPolicy.WebhookRetryDelays[retryIndex] : clock.UtcNow;
            
            await storeService.MarkPaymentEventFailedAsync(claimed.ProviderEventId, workerId, retryable, availableAtUtc, BillingErrorSanitizer.Sanitize(exception.Message), cancellationToken);
        }

        return true;
    }
}