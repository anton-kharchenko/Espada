using Espada.Api.Contracts.Constants;
using System.ComponentModel.DataAnnotations;

namespace Espada.Api.Contracts.Requests.Billing;

public sealed record CreateCheckoutRequest(string Plan) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!BillingPlanContractNames.IsSupported(Plan))
        {
            yield return new ValidationResult($"Plan must be {BillingPlanContractNames.Solo} or {BillingPlanContractNames.Team}.", [nameof(Plan)]);
        }
    }
}