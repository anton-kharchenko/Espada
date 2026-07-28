using Espada.Billing.Constants;
using FluentValidation;

namespace Espada.Billing.UseCases.Portal;

internal sealed class CreateCustomerPortalCommandValidator
    : AbstractValidator<CreateCustomerPortalCommand>
{
    public CreateCustomerPortalCommandValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.IdempotencyKey)
            .NotEmpty()
            .MaximumLength(BillingRequestLimitConstnts.MaximumIdempotencyKeyLength);
    }
}