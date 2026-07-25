namespace Espada.Application.UseCases.Artifacts.Commands.CreateArtifact
{
    public sealed record CreateArtifactResponse(
        Guid ArtifactId,
        Guid ArtifactRevisionId,
        int RevisionNumber,
        string ContentHash,
        int SizeInBytes,
        DateTimeOffset CreatedAtUtc);
}