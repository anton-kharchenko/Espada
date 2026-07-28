using Espada.Application.Contracts.Messaging;
using Espada.Billing.ApplicationErrors;
using Espada.Billing.Contracts;
using Espada.Billing.Models;
using Espada.Billing.Options;
using Espada.Domain.Rules;
using Microsoft.Extensions.Options;
using Stripe;

namespace Espada.Billing.UseCases.Webhook
{
    internal sealed class AcceptStripeWebhookCommandHandler(
        IEnumerable<IStripeWebhookIngestor> webhookIngestors,
        IOptions<BillingOptions> options) : ICommandHandler<AcceptStripeWebhookCommand, StripeWebhookReceipt>
    {
        public async Task<DomainResult<StripeWebhookReceipt>> Handle(AcceptStripeWebhookCommand request,
            CancellationToken cancellationToken)
        {
            IStripeWebhookIngestor? ingestor = webhookIngestors.SingleOrDefault();
            if (!options.Value.Enabled || ingestor is null)
            {
                return DomainResult.Failure<StripeWebhookReceipt>(BillingApplicationErrors.Unavailable);
            }

            try
            {
                bool inserted = await ingestor.AcceptAsync(request.Payload, request.Signature, cancellationToken);
                return DomainResult.Success(new StripeWebhookReceipt(true, !inserted));
            }
            catch (StripeException)
            {
                return DomainResult.Failure<StripeWebhookReceipt>(BillingApplicationErrors.InvalidWebhook);
            }
        }
    }
}