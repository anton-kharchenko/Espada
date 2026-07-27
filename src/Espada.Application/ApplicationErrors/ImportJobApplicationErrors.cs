using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors;

public static class ImportJobApplicationErrors
{
    public static readonly DomainError InvalidId = new(
        "ImportJob.Id.Invalid",
        "Import job ID cannot be empty.");

    public static DomainError NotFound(Guid importJobId) =>
        new(
            "ImportJob.NotFound",
            $"Import job with ID '{importJobId:D}' was not found.");

    public static DomainError NotFoundInWorkspace(
        Guid importJobId,
        Guid workspaceId) =>
        new(
            "ImportJob.NotFoundInWorkspace",
            $"Import job with ID '{importJobId:D}' was not found in workspace '{workspaceId:D}'.");

    public static readonly DomainError InvalidArtifactId = new(
        "ImportJob.ArtifactId.Invalid",
        "Artifact ID cannot be empty.");

    public static readonly DomainError InvalidArtifactRevisionId = new(
        "ImportJob.ArtifactRevisionId.Invalid",
        "Artifact revision ID cannot be empty.");

    public static readonly DomainError IdempotencyConflict = new(
        "ImportJob.Idempotency.Conflict",
        "The idempotency key was already used with a different request payload.");

    public static readonly DomainError EmbeddingModelRequired = new(
        "ImportJob.EmbeddingModel.Required",
        "An embedding model must be configured by the request or deployment.");

    public static DomainError CloudImportBlocked(string reason) =>
        new("ImportJob.CloudImport.Blocked", reason);
}