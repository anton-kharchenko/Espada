using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class Binding : AggregateRoot<BindingId>
    {
        public const int RepositoryCanonicalUriMaxLength = 2048;
        public const int RepositoryRelativePathPrefixMaxLength = 2000;
        public const int BranchMaxLength = 500;
        public const int AgentMaxLength = 100;

        private Binding()
        {
        }

        private Binding(BindingId id, ArtifactRevisionId artifactRevisionId, OrganizationId? organizationId,
            WorkspaceId workspaceId, ProjectId? projectId, string? repositoryCanonicalUri,
            string? repositoryRelativePathPrefix, string? branch, TaskId? taskId, string? agent,
            DateTimeOffset createdAtUtc) : base(id)
        {
            ArtifactRevisionId = artifactRevisionId;
            OrganizationId = organizationId;
            WorkspaceId = workspaceId;
            ProjectId = projectId;
            RepositoryCanonicalUri = repositoryCanonicalUri;
            RepositoryRelativePathPrefix = repositoryRelativePathPrefix;
            Branch = branch;
            TaskId = taskId;
            Agent = agent;
            CreatedAtUtc = createdAtUtc;
        }

        public ArtifactRevisionId ArtifactRevisionId { get; private set; } = null!;
        public OrganizationId? OrganizationId { get; private set; }
        public WorkspaceId WorkspaceId { get; private set; } = null!;
        public ProjectId? ProjectId { get; private set; }
        public string? RepositoryCanonicalUri { get; private set; }
        public string? RepositoryRelativePathPrefix { get; private set; }
        public string? Branch { get; private set; }
        public TaskId? TaskId { get; private set; }
        public string? Agent { get; private set; }
        public DateTimeOffset CreatedAtUtc { get; private set; }

        internal static DomainResult<Binding> Create(BindingId id, ArtifactRevision revision, Workspace workspace,
            OrganizationId? organizationId, Project? project, string? repositoryCanonicalUri,
            string? repositoryRelativePathPrefix, string? branch, ProjectTask? task, string? agent,
            DateTimeOffset createdAtUtc)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(revision);
            ArgumentNullException.ThrowIfNull(workspace);
            if (!workspace.Id.Equals(revision.WorkspaceId))
            {
                return DomainResult<Binding>.Failure(BindingErrors.WorkspaceMismatch);
            }

            if (organizationId is not null && !organizationId.Equals(workspace.OrganizationId))
            {
                return DomainResult<Binding>.Failure(BindingErrors.OrganizationWorkspaceMismatch);
            }

            if (project is not null && !project.WorkspaceId.Equals(revision.WorkspaceId))
            {
                return DomainResult<Binding>.Failure(BindingErrors.ProjectWorkspaceMismatch);
            }

            if (task is not null && project is null)
            {
                return DomainResult<Binding>.Failure(BindingErrors.TaskRequiresProject);
            }

            if (task is not null && !task.WorkspaceId.Equals(revision.WorkspaceId))
            {
                return DomainResult<Binding>.Failure(BindingErrors.TaskWorkspaceMismatch);
            }

            if (task is not null && !task.ProjectId.Equals(project!.Id))
            {
                return DomainResult<Binding>.Failure(BindingErrors.TaskProjectMismatch);
            }

            string? normalizedRepositoryUri = Normalize(repositoryCanonicalUri);
            string? normalizedPath = Normalize(repositoryRelativePathPrefix);
            string? normalizedBranch = Normalize(branch);
            string? normalizedAgent = Normalize(agent);
            if (normalizedRepositoryUri?.Length > RepositoryCanonicalUriMaxLength)
            {
                return DomainResult<Binding>.Failure(BindingErrors.RepositoryCanonicalUriTooLong);
            }

            if (normalizedPath?.Length > RepositoryRelativePathPrefixMaxLength)
            {
                return DomainResult<Binding>.Failure(BindingErrors.RepositoryRelativePathTooLong);
            }

            if (normalizedBranch?.Length > BranchMaxLength)
            {
                return DomainResult<Binding>.Failure(BindingErrors.BranchTooLong);
            }

            if (normalizedAgent?.Length > AgentMaxLength)
            {
                return DomainResult<Binding>.Failure(BindingErrors.AgentTooLong);
            }

            if (normalizedPath is not null)
            {
                string[] segments = normalizedPath.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (Path.IsPathRooted(normalizedPath) || segments.Any(segment => segment is "." or ".."))
                {
                    return DomainResult<Binding>.Failure(BindingErrors.RepositoryRelativePathInvalid);
                }

                normalizedPath = string.Join('/', segments);
            }

            return DomainResult<Binding>.Success(new Binding(id, revision.Id, organizationId, revision.WorkspaceId,
                project?.Id, normalizedRepositoryUri, normalizedPath, normalizedBranch, task?.Id, normalizedAgent,
                createdAtUtc));
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}