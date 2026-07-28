namespace Espada.Application.Models
{
    public sealed record ContextConflict(
        string RuleKey,
        string ConflictCode,
        IReadOnlyList<Guid> ArtifactIds,
        Guid? WinnerArtifactId,
        string Explanation);
}