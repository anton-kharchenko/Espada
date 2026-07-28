using Espada.Billing.Constants;
using FluentValidation;

namespace Espada.Billing.UseCases.Webhook
{
    internal sealed class AcceptStripeWebhookCommandValidator : AbstractValidator<AcceptStripeWebhookCommand>
    {
        public AcceptStripeWebhookCommandValidator()
        {
            RuleFor(command => command.Payload)
                .NotEmpty()
                .MaximumLength(BillingRequestLimitConstants.MaximumWebhookPayloadBytes);

            RuleFor(command => command.Signature)
                .NotEmpty()
                .MaximumLength(BillingRequestLimitConstants.MaximumWebhookSignatureLength);
        }
    }
}