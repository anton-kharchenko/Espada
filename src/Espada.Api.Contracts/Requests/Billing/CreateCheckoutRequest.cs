using Espada.Api.Contracts.Constants;
using System.ComponentModel.DataAnnotations;

namespace Espada.Api.Contracts.Requests.Billing;

public sealed record CreateCheckoutRequest(string Plan) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!BillingPlanConstants.IsSupported(Plan))
        {
            yield return new ValidationResult($"Plan must be {BillingPlanConstants.Solo} or {BillingPlanConstants.Team}.", [nameof(Plan)]);
        }
    }
}