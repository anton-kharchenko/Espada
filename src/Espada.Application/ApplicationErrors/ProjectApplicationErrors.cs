using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors
{
    public static class ProjectApplicationErrors
    {
        public static readonly DomainError InvalidId = new(
            "Project.Id.Invalid",
            "Project ID cannot be empty.");

        public static DomainError NotFound(Guid projectId)
        {
            return new DomainError(
                "Project.NotFound",
                $"Project with ID '{projectId:D}' was not found.");
        }

        public static DomainError NotFoundInWorkspace(Guid projectId, Guid workspaceId)
        {
            return new DomainError(
                "Project.NotFoundInWorkspace",
                $"Project with ID '{projectId:D}' was not found in workspace '{workspaceId:D}'.");
        }

        public static DomainError DuplicateCanonicalRemoteUri(string canonicalRemoteUri)
        {
            return new DomainError(
                "Project.CanonicalRemoteUri.Duplicate",
                $"Project canonical remote URI '{canonicalRemoteUri}' already exists in the workspace.");
        }
    }
}