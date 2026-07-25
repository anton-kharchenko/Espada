namespace Espada.Application.UseCases.Artifacts.Queries.ListArtifactRevisions
{
    public sealed record ListArtifactRevisionsResponse(
        IReadOnlyList<ArtifactRevisionListItemResponse> Items);
}