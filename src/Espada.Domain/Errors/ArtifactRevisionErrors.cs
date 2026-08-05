using Espada.Domain.Rules;

namespace Espada.Domain.Errors
{
    public static class ArtifactRevisionErrors
    {
        public static readonly DomainError ContentEmpty =
            new("ArtifactRevision.Content.Empty", "Artifact revision content cannot be empty.");

        public static readonly DomainError InvalidRevisionNumber = new("ArtifactRevision.Number.Invalid",
            "Artifact revision number must be greater than zero.");

        public static readonly DomainError ArtifactArchived = new("ArtifactRevision.Artifact.Archived",
            "A revision cannot be added to an archived artifact.");
    }
}