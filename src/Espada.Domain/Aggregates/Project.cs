using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class Project : AggregateRoot<ProjectId>, IHasConcurrencyVersion
    {
        public const int NameMaxLength = 200;
        public const int CanonicalRemoteUriMaxLength = 2048;

        private Project()
        {
        }

        private Project(ProjectId id, WorkspaceId workspaceId, string name, string canonicalRemoteUri,
            string[] localAliases, DateTimeOffset createdAtUtc) : base(id)
        {
            WorkspaceId = workspaceId;
            Name = name;
            CanonicalRemoteUri = canonicalRemoteUri;
            LocalAliases = localAliases;
            CreatedAtUtc = createdAtUtc;
            UpdatedAtUtc = createdAtUtc;
        }

        public WorkspaceId WorkspaceId { get; private set; } = null!;
        public string Name { get; private set; } = string.Empty;
        public string CanonicalRemoteUri { get; private set; } = string.Empty;
        public string[] LocalAliases { get; private set; } = [];
        public DateTimeOffset CreatedAtUtc { get; private set; }
        public DateTimeOffset UpdatedAtUtc { get; private set; }
        public uint Version { get; private set; }

        public static DomainResult<Project> Create(ProjectId id, WorkspaceId workspaceId, string? name,
            string? canonicalRemoteUri, IEnumerable<string>? localAliases, DateTimeOffset createdAtUtc)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(workspaceId);
            if (string.IsNullOrWhiteSpace(name))
            {
                return DomainResult<Project>.Failure(ProjectErrors.NameEmpty);
            }

            string normalizedName = name.Trim();
            if (normalizedName.Length > NameMaxLength)
            {
                return DomainResult<Project>.Failure(ProjectErrors.NameTooLong);
            }

            if (string.IsNullOrWhiteSpace(canonicalRemoteUri))
            {
                return DomainResult<Project>.Failure(ProjectErrors.CanonicalRemoteUriEmpty);
            }

            string normalizedRemoteUri = canonicalRemoteUri.Trim();
            if (normalizedRemoteUri.Length > CanonicalRemoteUriMaxLength)
            {
                return DomainResult<Project>.Failure(ProjectErrors.CanonicalRemoteUriTooLong);
            }

            string[] aliases = (localAliases ?? []).ToArray();
            if (aliases.Any(string.IsNullOrWhiteSpace))
            {
                return DomainResult<Project>.Failure(ProjectErrors.LocalAliasEmpty);
            }

            string[] normalizedAliases =
                aliases.Select(alias => alias.Trim()).Distinct(StringComparer.Ordinal).ToArray();
            return DomainResult<Project>.Success(new Project(id, workspaceId, normalizedName, normalizedRemoteUri,
                normalizedAliases, createdAtUtc));
        }

        public DomainResult<ProjectTask> CreateTask(TaskId taskId, string? title, DateTimeOffset createdAtUtc)
        {
            return ProjectTask.Create(taskId, this, title, createdAtUtc);
        }
    }
}