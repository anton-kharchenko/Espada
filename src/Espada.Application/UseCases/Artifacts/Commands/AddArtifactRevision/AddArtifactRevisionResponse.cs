namespace Espada.Application.UseCases.Artifacts.Commands.AddArtifactRevision
{
    public sealed record AddArtifactRevisionResponse(
        Guid ArtifactId,
        Guid ArtifactRevisionId,
        int RevisionNumber,
        string ContentHash,
        int SizeInBytes,
        DateTimeOffset CreatedAtUtc);
}