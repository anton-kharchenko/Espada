using Espada.Billing.Constants;
using FluentValidation;

namespace Espada.Billing.UseCases.Checkout
{
    internal sealed class CreateCheckoutCommandValidator : AbstractValidator<CreateCheckoutCommand>
    {
        public CreateCheckoutCommandValidator()
        {
            RuleFor(command => command.WorkspaceId)
                .NotEmpty();

            RuleFor(command => command.Plan)
                .IsInEnum();

            RuleFor(command => command.IdempotencyKey)
                .NotEmpty()
                .MaximumLength(BillingRequestLimitConstants.MaximumIdempotencyKeyLength);
        }
    }
}