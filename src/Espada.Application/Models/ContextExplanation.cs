namespace Espada.Application.Models
{
    public sealed record ContextExplanation(
        Guid BindingId,
        Guid ArtifactId,
        Guid RevisionId,
        string DecisionCode,
        string Explanation);
}