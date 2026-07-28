namespace Espada.Application.UseCases.Context.Queries.BuildContext
{
    public sealed record BuildContextResponse(
        Guid WorkspaceId,
        Guid? OrganizationId,
        Guid? ProjectId,
        Guid? TaskId,
        string? RepositoryCanonicalUri,
        string? RepositoryRelativePath,
        string? Branch,
        string Agent,
        IReadOnlyList<ContextItemResponse> IncludedItems,
        IReadOnlyList<ContextItemResponse> ExcludedItems,
        IReadOnlyList<ContextConflictResponse> Conflicts,
        IReadOnlyList<ContextExplanationResponse> Explanations,
        ContextBudgetSummaryResponse Budget);
}