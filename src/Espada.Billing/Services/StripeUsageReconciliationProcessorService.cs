using Espada.Application.Contracts.Time;
using Espada.Billing.Constants;
using Espada.Billing.Contracts;
using Espada.Billing.Helpers;
using Espada.Billing.Models;
using Espada.Billing.Options;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Billing;
using System.Globalization;

namespace Espada.Billing.Services
{
    internal sealed class StripeUsageReconciliationProcessorService(
        StripeClient client,
        IBillingStoreService storeService,
        IOptions<BillingOptions> options,
        IClockService clock) : IUsageReconciliationProcessor
    {
        public async Task<bool> ProcessNextAsync(string workerId, CancellationToken cancellationToken = default)
        {
            ClaimedUsageReconciliation? usage = await storeService.ClaimUsageReconciliationAsync(workerId,
                BillingProcessingConstants.LeaseDuration, cancellationToken);
            if (usage is null)
            {
                return false;
            }

            try
            {
                string idempotencyKey = usage.EventId.ToString(BillingProcessingConstants.CompactGuidFormat);
                await client.V1.Billing.MeterEvents.CreateAsync(
                    new MeterEventCreateOptions
                    {
                        EventName = options.Value.Usage.GetEventName(usage.Metric),
                        Identifier = idempotencyKey,
                        Timestamp = usage.OccurredAtUtc.UtcDateTime,
                        Payload = new Dictionary<string, string>
                        {
                            [StripeMetadataKeyConstants.CustomerId] =
                                usage.ProviderCustomerId,
                            [StripeMetadataKeyConstants.UsageValue] =
                                usage.Quantity.ToString(CultureInfo.InvariantCulture)
                        }
                    },
                    new RequestOptions { IdempotencyKey = idempotencyKey },
                    cancellationToken);
                await storeService.MarkUsageReconciledAsync(usage.EventId, workerId, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                bool retryable =
                    exception is StripeException or HttpRequestException or IOException or TimeoutException;
                retryable &= usage.Attempt <= BillingProcessingConstants.MaximumRetryAttempts;
                DateTimeOffset availableAtUtc = retryable
                    ? clock.UtcNow + BillingProcessingConstants.GetUsageRetryDelay(usage.Attempt)
                    : clock.UtcNow;
                await storeService.MarkUsageReconciliationFailedAsync(usage.EventId, workerId, retryable,
                    availableAtUtc, BillingErrorSanitizerHelper.Sanitize(exception.Message), cancellationToken);
            }

            return true;
        }
    }
}