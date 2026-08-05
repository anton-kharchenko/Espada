using Espada.Domain.Aggregates;

namespace Espada.Application.Models
{
    public sealed record ResolvedContext(
        Workspace Workspace,
        Project? Project,
        ProjectTask? Task,
        string? RepositoryCanonicalUri,
        string? RepositoryRelativePath,
        string? Branch,
        string Agent,
        IReadOnlyList<ResolvedContextItem> IncludedItems,
        IReadOnlyList<ResolvedContextItem> ExcludedItems,
        IReadOnlyList<ContextConflict> Conflicts,
        IReadOnlyList<ContextExplanation> Explanations,
        ContextBudgetSummary Budget);
}