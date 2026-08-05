using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Errors
{
    public static class ArtifactErrors
    {
        public static readonly DomainError TitleEmpty = new("Artifact.Title.Empty", "Artifact title cannot be empty.");

        public static readonly DomainError TitleTooLong = new("Artifact.Title.TooLong",
            $"Artifact title cannot exceed {ArtifactTitle.MaxLength} characters.");

        public static readonly DomainError AlreadyArchived =
            new("Artifact.AlreadyArchived", "Artifact is already archived.");

        public static readonly DomainError ArchivedArtifactCannotBeRenamed =
            new("Artifact.ArchivedCannotBeRenamed", "An archived artifact cannot be renamed.");

        public static readonly DomainError ArchivedArtifactCannotChangePriority =
            new("Artifact.ArchivedCannotChangePriority", "An archived artifact priority cannot be changed.");
    }
}