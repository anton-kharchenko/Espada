using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors
{
    public static class ArtifactRevisionApplicationErrors
    {
        public static readonly DomainError InvalidId = new(
            "ArtifactRevision.Id.Invalid",
            "Artifact revision ID cannot be empty.");

        public static DomainError NotFound(Guid artifactRevisionId)
        {
            return new DomainError(
                "ArtifactRevision.NotFound",
                $"Artifact revision with ID '{artifactRevisionId:D}' was not found.");
        }

        public static DomainError NotFoundInArtifact(
            Guid artifactRevisionId,
            Guid artifactId)
        {
            return new DomainError(
                "ArtifactRevision.NotFoundInArtifact",
                $"Artifact revision with ID '{artifactRevisionId:D}' was not found in artifact '{artifactId:D}'.");
        }
    }
}