using Espada.Billing.Constants;
using Espada.Billing.Contracts;
using Espada.Billing.Enums;
using Espada.Billing.Models;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Espada.Billing;

internal sealed class StripeBillingProvider(
    StripeClient client,
    IOptions<BillingOptions> options) : IStripeBillingProvider
{
    private readonly BillingOptions _options = options.Value;

    public async Task<HostedBillingSession> CreateCheckoutAsync(
        Guid workspaceId,
        string? customerId,
        CloudBillingPlanType plan,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        if (customerId is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        string priceId = plan switch
        {
            CloudBillingPlanType.Solo => _options.Solo.PriceId,
            CloudBillingPlanType.Team => _options.Team.PriceId,
            _ => throw new ArgumentOutOfRangeException(nameof(plan))
        };

        Session session = await client.V1.Checkout.Sessions.CreateAsync(
            new SessionCreateOptions
            {
                Mode = StripeCheckoutModeConstants.Subscription,
                Customer = customerId,
                ClientReferenceId = workspaceId.ToString(BillingProcessingPolicy.DefaultGuidFormat),
                SuccessUrl = _options.CheckoutSuccessUrl!.AbsoluteUri,
                CancelUrl = _options.CheckoutCancelUrl!.AbsoluteUri,
                LineItems =
                [
                    new SessionLineItemOptions { Price = priceId, Quantity = 1 }
                ],
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        [StripeMetadataKeyContants.WorkspaceId] = workspaceId.ToString(BillingProcessingPolicy.DefaultGuidFormat),
                        [StripeMetadataKeyContants.Plan] = plan.ToString()
                    }
                }
            },
            new RequestOptions { IdempotencyKey = idempotencyKey },
            cancellationToken);

        return new HostedBillingSession(
            session.Id,
            new Uri(session.Url, UriKind.Absolute));
    }

    public async Task<HostedBillingSession> CreateCustomerPortalAsync(
        string customerId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        Stripe.BillingPortal.Session session = await client.V1.BillingPortal.Sessions.CreateAsync(
                new Stripe.BillingPortal.SessionCreateOptions
                {
                    Customer = customerId,
                    ReturnUrl = _options.PortalReturnUrl!.AbsoluteUri
                },
                new RequestOptions { IdempotencyKey = idempotencyKey },
                cancellationToken);
        return new HostedBillingSession(
            session.Id,
            new Uri(session.Url, UriKind.Absolute));
    }
}