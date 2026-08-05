namespace Espada.Application.UseCases.Artifacts.Queries.GetArtifactById
{
    public sealed record CurrentArtifactRevisionResponse(
        Guid Id,
        int Number,
        string Content,
        string ContentHash,
        int SizeInBytes,
        DateTimeOffset CreatedAtUtc);
}