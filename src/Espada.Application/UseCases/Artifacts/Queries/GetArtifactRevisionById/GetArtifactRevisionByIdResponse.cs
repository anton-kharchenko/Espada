namespace Espada.Application.UseCases.Artifacts.Queries.GetArtifactRevisionById
{
    public sealed record GetArtifactRevisionByIdResponse(
        Guid Id,
        Guid ArtifactId,
        int Number,
        string Content,
        string ContentHash,
        int SizeInBytes,
        DateTimeOffset CreatedAtUtc);
}