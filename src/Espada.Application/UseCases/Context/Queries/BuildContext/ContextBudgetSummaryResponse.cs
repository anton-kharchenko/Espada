namespace Espada.Application.UseCases.Context.Queries.BuildContext
{
    public sealed record ContextBudgetSummaryResponse(
        int RequestedBytes,
        int HardPolicyBytes,
        int UsedBytes,
        int RemainingBytes,
        int IncludedItemCount,
        int ExcludedItemCount);
}