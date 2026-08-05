namespace Espada.Application.Models
{
    public sealed record ContextBudgetSummary(
        int RequestedBytes,
        int HardPolicyBytes,
        int UsedBytes,
        int RemainingBytes,
        int IncludedItemCount,
        int ExcludedItemCount);
}