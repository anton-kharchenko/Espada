using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Events;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class Workspace : AggregateRoot<WorkspaceId>, IHasConcurrencyVersion
    {
        private Workspace()
        {
        }

        private Workspace(
            WorkspaceId id,
            WorkspaceName name,
            WorkspaceType type,
            OrganizationId? organizationId,
            DateTimeOffset createdAtUtc)
            : base(id)
        {
            Name = name;
            Type = type;
            OrganizationId = organizationId;
            CreatedAtUtc = createdAtUtc;
            Status = WorkspaceStatusType.Active;
        }

        public WorkspaceName Name { get; } = null!;

        public WorkspaceType Type { get; private set; } = null!;

        public OrganizationId? OrganizationId { get; private set; }

        public WorkspaceStatusType Status { get; private set; } = null!;

        public DateTimeOffset CreatedAtUtc { get; private set; }

        public DateTimeOffset? ArchivedAtUtc { get; private set; }
        public uint Version { get; private set; }

        public static DomainResult<Workspace> Create(
            WorkspaceId id,
            WorkspaceName name,
            WorkspaceType type,
            OrganizationId? organizationId,
            DateTimeOffset createdAtUtc)
        {
            Workspace workspace = new(id, name, type, organizationId, createdAtUtc);

            workspace.RaiseDomainEvent(
                new WorkspaceCreatedDomainEvent(workspace.Id, workspace.Name.Value, createdAtUtc));

            return DomainResult.Success(workspace);
        }

        public DomainResult Archive(DateTimeOffset archivedAtUtc)
        {
            if (Status.Equals(WorkspaceStatusType.Archived))
            {
                return DomainResult.Failure(WorkspaceErrors.AlreadyArchived);
            }

            Status = WorkspaceStatusType.Archived;
            ArchivedAtUtc = archivedAtUtc;

            RaiseDomainEvent(
                new WorkspaceArchivedDomainEvent(
                    Id,
                    archivedAtUtc));

            return DomainResult.Success();
        }
    }
}