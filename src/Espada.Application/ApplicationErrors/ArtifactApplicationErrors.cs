using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors
{
    public static class ArtifactApplicationErrors
    {
        public static readonly DomainError InvalidId = new("Artifact.Id.Invalid", "Artifact ID cannot be empty.");

        public static DomainError UnsupportedType(int typeId) => new("Artifact.Type.Unsupported", $"Artifact type with ID '{typeId}' is not supported.");

        public static DomainError NotFound(Guid artifactId) => new("Artifact.NotFound", $"Artifact with ID '{artifactId:D}' was not found.");

        public static DomainError NotFoundInWorkspace(Guid artifactId, Guid workspaceId) => new("Artifact.NotFoundInWorkspace", $"Artifact with ID '{artifactId:D}' was not found in workspace '{workspaceId:D}'.");
    }
}