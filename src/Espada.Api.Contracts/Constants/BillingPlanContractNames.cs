namespace Espada.Api.Contracts.Constants;

public static class BillingPlanContractNames
{
    public const string Solo = "Solo";
    public const string Team = "Team";

    public static bool IsSupported(string? value) => value is not null && (value.Equals(Solo, StringComparison.OrdinalIgnoreCase) || value.Equals(Team, StringComparison.OrdinalIgnoreCase));
}