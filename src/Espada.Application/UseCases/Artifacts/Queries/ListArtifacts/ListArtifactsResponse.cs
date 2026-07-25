namespace Espada.Application.UseCases.Artifacts.Queries.ListArtifacts
{
    public sealed record ListArtifactsResponse(
        IReadOnlyList<ArtifactListItemResponse> Items);
}