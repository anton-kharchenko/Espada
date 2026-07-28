using Espada.Domain.Rules;

namespace Espada.Domain.Errors
{
    public static class ProjectErrors
    {
        public static DomainError NameEmpty { get; } = new("Project.NameEmpty", "Project name cannot be empty.");

        public static DomainError NameTooLong { get; } =
            new("Project.NameTooLong", "Project name cannot exceed 200 characters.");

        public static DomainError CanonicalRemoteUriEmpty { get; } = new("Project.CanonicalRemoteUriEmpty",
            "Canonical remote URI cannot be empty.");

        public static DomainError CanonicalRemoteUriTooLong { get; } = new("Project.CanonicalRemoteUriTooLong",
            "Canonical remote URI cannot exceed 2048 characters.");

        public static DomainError LocalAliasEmpty { get; } =
            new("Project.LocalAliasEmpty", "Local aliases cannot be empty.");
    }
}