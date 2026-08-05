using Espada.Domain.Aggregates;

namespace Espada.Application.Models
{
    public sealed record ResolvedContextItem(
        Binding Binding,
        Artifact Artifact,
        ArtifactRevision Revision,
        string? RuleKey,
        string? Enforcement,
        string Content,
        int RulePriority,
        MemoryMetadata? MemoryMetadata,
        ContextSpecificity Specificity,
        IReadOnlyList<ContextSelectorMatch> Selectors,
        int SizeInBytes,
        string DecisionCode,
        string Explanation);
}