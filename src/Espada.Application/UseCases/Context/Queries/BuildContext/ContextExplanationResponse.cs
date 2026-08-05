namespace Espada.Application.UseCases.Context.Queries.BuildContext
{
    public sealed record ContextExplanationResponse(
        Guid BindingId,
        Guid ArtifactId,
        Guid RevisionId,
        string DecisionCode,
        string Explanation);
}