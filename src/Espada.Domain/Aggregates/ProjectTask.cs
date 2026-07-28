using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class ProjectTask : AggregateRoot<TaskId>, IHasConcurrencyVersion
    {
        public const int TitleMaxLength = 500;

        private ProjectTask()
        {
        }

        private ProjectTask(TaskId id, WorkspaceId workspaceId, ProjectId projectId, string title,
            DateTimeOffset createdAtUtc) : base(id)
        {
            WorkspaceId = workspaceId;
            ProjectId = projectId;
            Title = title;
            Status = TaskStatusType.Active;
            CreatedAtUtc = createdAtUtc;
            UpdatedAtUtc = createdAtUtc;
        }

        public WorkspaceId WorkspaceId { get; private set; } = null!;
        public ProjectId ProjectId { get; private set; } = null!;
        public string Title { get; private set; } = string.Empty;
        public TaskStatusType Status { get; private set; } = null!;
        public DateTimeOffset CreatedAtUtc { get; private set; }
        public DateTimeOffset UpdatedAtUtc { get; private set; }
        public DateTimeOffset? CompletedAtUtc { get; private set; }
        public DateTimeOffset? ArchivedAtUtc { get; private set; }
        public uint Version { get; private set; }

        internal static DomainResult<ProjectTask> Create(TaskId id, Project project, string? title,
            DateTimeOffset createdAtUtc)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(project);
            if (string.IsNullOrWhiteSpace(title))
            {
                return DomainResult<ProjectTask>.Failure(TaskErrors.TitleEmpty);
            }

            string normalizedTitle = title.Trim();
            return normalizedTitle.Length > TitleMaxLength
                ? DomainResult<ProjectTask>.Failure(TaskErrors.TitleTooLong)
                : DomainResult<ProjectTask>.Success(new ProjectTask(id, project.WorkspaceId, project.Id,
                    normalizedTitle, createdAtUtc));
        }

        public DomainResult Complete(DateTimeOffset completedAtUtc)
        {
            if (!Status.Equals(TaskStatusType.Active))
            {
                return DomainResult.Failure(TaskErrors.NotActive);
            }

            Status = TaskStatusType.Completed;
            CompletedAtUtc = completedAtUtc;
            UpdatedAtUtc = completedAtUtc;
            return DomainResult.Success();
        }

        public DomainResult Archive(DateTimeOffset archivedAtUtc)
        {
            if (Status.Equals(TaskStatusType.Archived))
            {
                return DomainResult.Failure(TaskErrors.AlreadyArchived);
            }

            Status = TaskStatusType.Archived;
            ArchivedAtUtc = archivedAtUtc;
            UpdatedAtUtc = archivedAtUtc;
            return DomainResult.Success();
        }
    }
}