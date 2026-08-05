using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors
{
    public static class ImportJobApplicationErrors
    {
        public static readonly DomainError InvalidId = new(
            "ImportJob.Id.Invalid",
            "Import job ID cannot be empty.");

        public static readonly DomainError InvalidArtifactId = new(
            "ImportJob.ArtifactId.Invalid",
            "Artifact ID cannot be empty.");

        public static readonly DomainError InvalidArtifactRevisionId = new(
            "ImportJob.ArtifactRevisionId.Invalid",
            "Artifact revision ID cannot be empty.");

        public static readonly DomainError IdempotencyConflict = new(
            "ImportJob.Idempotency.Conflict",
            "The idempotency key was already used with a different request payload.");

        public static readonly DomainError RepositoryIdentityInvalid = new(
            "ImportJob.Repository.IdentityInvalid",
            "Repository source identity must contain its project ID.");

        public static readonly DomainError RepositoryRootUnavailable = new(
            "ImportJob.Repository.RootUnavailable",
            "No accessible local path is registered for the repository project.");

        public static DomainError NotFound(Guid importJobId)
        {
            return new DomainError(
                "ImportJob.NotFound",
                $"Import job with ID '{importJobId:D}' was not found.");
        }

        public static DomainError NotFoundInWorkspace(
            Guid importJobId,
            Guid workspaceId)
        {
            return new DomainError(
                "ImportJob.NotFoundInWorkspace",
                $"Import job with ID '{importJobId:D}' was not found in workspace '{workspaceId:D}'.");
        }

        public static DomainError CloudImportBlocked(string reason)
        {
            return new DomainError("ImportJob.CloudImport.Blocked", reason);
        }
    }
}