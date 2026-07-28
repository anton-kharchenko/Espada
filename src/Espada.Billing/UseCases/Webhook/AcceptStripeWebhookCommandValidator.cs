using Espada.Billing.Constants;
using FluentValidation;

namespace Espada.Billing.UseCases.Webhook;

internal sealed class AcceptStripeWebhookCommandValidator : AbstractValidator<AcceptStripeWebhookCommand>
{
    public AcceptStripeWebhookCommandValidator()
    {
        RuleFor(command => command.Payload)
            .NotEmpty()
            .MaximumLength(BillingRequestLimitConstnts.MaximumWebhookPayloadBytes);

        RuleFor(command => command.Signature)
            .NotEmpty()
            .MaximumLength(BillingRequestLimitConstnts.MaximumWebhookSignatureLength);
    }
}