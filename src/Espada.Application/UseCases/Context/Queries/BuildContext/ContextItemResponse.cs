using Espada.Application.UseCases.Memories.Queries.SearchMemory;

namespace Espada.Application.UseCases.Context.Queries.BuildContext
{
    public sealed record ContextItemResponse(
        Guid BindingId,
        Guid ArtifactId,
        Guid RevisionId,
        string ArtifactKind,
        string Title,
        string? RuleKey,
        string? Enforcement,
        string Content,
        int RulePriority,
        int ArtifactPriority,
        bool? UserConfirmed,
        decimal? Confidence,
        MemoryProvenanceResponse? Provenance,
        ContextSpecificityResponse Specificity,
        IReadOnlyList<ContextSelectorMatchResponse> Selectors,
        int SizeInBytes,
        string DecisionCode);
}