namespace Espada.Application.UseCases.Context.Queries.BuildContext
{
    public sealed record ContextConflictResponse(
        string RuleKey,
        string ConflictCode,
        IReadOnlyList<Guid> ArtifactIds,
        Guid? WinnerArtifactId,
        string Explanation);
}