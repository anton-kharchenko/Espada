namespace Espada.Application.UseCases.Artifacts.Queries.ListArtifactRevisions
{
    public sealed record ArtifactRevisionListItemResponse(
        Guid Id,
        int Number,
        string ContentHash,
        int SizeInBytes,
        DateTimeOffset CreatedAtUtc);
}