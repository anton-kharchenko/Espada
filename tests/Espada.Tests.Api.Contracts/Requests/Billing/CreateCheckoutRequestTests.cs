using Espada.Api.Contracts.Constants;
using Espada.Api.Contracts.Requests.Billing;
using Espada.Tests.Api.Contracts.Validation;
using System.ComponentModel.DataAnnotations;

namespace Espada.Tests.Api.Contracts.Requests.Billing;

public sealed class CreateCheckoutRequestTests
{
    public static TheoryData<string> SupportedPlans =>
    [
        BillingPlanConstants.Solo,
        BillingPlanConstants.Team
    ];

    public static TheoryData<string> UnsupportedPlans =>
    [
        string.Empty,
        "Enterprise",
        "Pro"
    ];

    [Theory]
    [MemberData(nameof(SupportedPlans))]
    public void Validate_WithSupportedPlan_ShouldSucceed(string plan)
    {
        CreateCheckoutRequest request = new(plan);

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.Empty(results);
    }

    [Theory]
    [MemberData(nameof(UnsupportedPlans))]
    public void Validate_WithUnsupportedPlan_ShouldReturnPlanError(string plan)
    {
        CreateCheckoutRequest request = new(plan);

        IReadOnlyList<ValidationResult> results = ValidationTestHelper.Validate(request);

        Assert.True(results.HasErrorFor(nameof(CreateCheckoutRequest.Plan)));
    }
}