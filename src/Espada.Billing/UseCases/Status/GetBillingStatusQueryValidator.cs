using FluentValidation;

namespace Espada.Billing.UseCases.Status
{
    internal sealed class GetBillingStatusQueryValidator : AbstractValidator<GetBillingStatusQuery>
    {
        public GetBillingStatusQueryValidator()
        {
            RuleFor(query => query.WorkspaceId)
                .NotEmpty();
        }
    }
}