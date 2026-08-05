using Espada.Domain.Rules;

namespace Espada.Domain.Errors
{
    public static class BindingErrors
    {
        public static DomainError RevisionMismatch { get; } =
            new("Binding.RevisionMismatch", "Binding revision must belong to the artifact.");

        public static DomainError WorkspaceMismatch { get; } = new("Binding.WorkspaceMismatch",
            "Binding workspace must match the artifact revision workspace.");

        public static DomainError OrganizationWorkspaceMismatch { get; } = new("Binding.OrganizationWorkspaceMismatch",
            "Binding organization must own the selected workspace.");

        public static DomainError ProjectWorkspaceMismatch { get; } = new("Binding.ProjectWorkspaceMismatch",
            "Binding project must belong to the artifact workspace.");

        public static DomainError TaskRequiresProject { get; } = new("Binding.TaskRequiresProject",
            "A task selector requires its project selector.");

        public static DomainError TaskWorkspaceMismatch { get; } = new("Binding.TaskWorkspaceMismatch",
            "Binding task must belong to the artifact workspace.");

        public static DomainError TaskProjectMismatch { get; } = new("Binding.TaskProjectMismatch",
            "Binding task must belong to the selected project.");

        public static DomainError RepositoryRelativePathInvalid { get; } = new("Binding.RepositoryRelativePathInvalid",
            "Repository-relative path prefix cannot be rooted or contain traversal segments.");

        public static DomainError RepositoryCanonicalUriTooLong { get; } = new("Binding.RepositoryCanonicalUriTooLong",
            "Repository canonical URI cannot exceed 2048 characters.");

        public static DomainError RepositoryRelativePathTooLong { get; } = new("Binding.RepositoryRelativePathTooLong",
            "Repository-relative path prefix cannot exceed 2000 characters.");

        public static DomainError BranchTooLong { get; } =
            new("Binding.BranchTooLong", "Branch selector cannot exceed 500 characters.");

        public static DomainError AgentTooLong { get; } =
            new("Binding.AgentTooLong", "Agent selector cannot exceed 100 characters.");
    }
}