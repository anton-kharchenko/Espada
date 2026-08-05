using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Events;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class Artifact : AggregateRoot<ArtifactId>, IHasConcurrencyVersion
    {
        private Artifact()
        {
        }

        private Artifact(
            ArtifactId id,
            WorkspaceId workspaceId,
            ArtifactTitle title,
            ArtifactKindType kindType,
            ArtifactType type,
            DateTimeOffset createdAtUtc)
            : base(id)
        {
            WorkspaceId = workspaceId;
            Title = title;
            KindType = kindType;
            Type = type;
            Status = ArtifactStatusType.Active;
            Priority = ContextPriority.Neutral;
            CreatedAtUtc = createdAtUtc;
            UpdatedAtUtc = createdAtUtc;
        }

        public int RevisionCount => CurrentRevisionNumber?.Value ?? 0;

        public WorkspaceId WorkspaceId { get; } = null!;

        public ArtifactTitle Title { get; private set; } = null!;

        public ArtifactKindType KindType { get; } = null!;

        public ArtifactType Type { get; } = null!;

        public ArtifactStatusType Status { get; private set; } = null!;

        public ContextPriority Priority { get; private set; } = ContextPriority.Neutral;

        public DateTimeOffset CreatedAtUtc { get; private set; }

        public ArtifactRevisionId? CurrentRevisionId { get; private set; }

        public RevisionNumber? CurrentRevisionNumber { get; private set; }

        public DateTimeOffset UpdatedAtUtc { get; private set; }

        public DateTimeOffset? ArchivedAtUtc { get; private set; }
        public uint Version { get; private set; }

        public static DomainResult<Artifact> Create(
            ArtifactId id,
            WorkspaceId workspaceId,
            ArtifactTitle title,
            ArtifactKindType kindType,
            ArtifactType type,
            DateTimeOffset createdAtUtc)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(workspaceId);
            ArgumentNullException.ThrowIfNull(title);
            ArgumentNullException.ThrowIfNull(kindType);
            ArgumentNullException.ThrowIfNull(type);

            Artifact artifact = new(id, workspaceId, title, kindType, type, createdAtUtc);

            artifact.RaiseDomainEvent(
                new ArtifactCreatedDomainEvent(
                    artifact.Id,
                    artifact.WorkspaceId,
                    artifact.Title.Value,
                    artifact.KindType,
                    artifact.Type,
                    createdAtUtc));

            return DomainResult<Artifact>.Success(artifact);
        }

        public static DomainResult<Artifact> CreateDraft(
            ArtifactId id,
            WorkspaceId workspaceId,
            ArtifactTitle title,
            ArtifactKindType kindType,
            ArtifactType type,
            DateTimeOffset createdAtUtc)
        {
            DomainResult<Artifact> result = Create(id, workspaceId, title, kindType, type, createdAtUtc);
            if (result.IsSuccess)
            {
                result.Value.Status = ArtifactStatusType.Draft;
            }

            return result;
        }

        public DomainResult Rename(ArtifactTitle title, DateTimeOffset renamedAtUtc)
        {
            ArgumentNullException.ThrowIfNull(title);

            if (Equals(Status, ArtifactStatusType.Archived))
            {
                return DomainResult.Failure(ArtifactErrors.ArchivedArtifactCannotBeRenamed);
            }

            if (Title == title)
            {
                return DomainResult.Success();
            }

            string previousTitle = Title.Value;

            Title = title;
            UpdatedAtUtc = renamedAtUtc;

            RaiseDomainEvent(new ArtifactRenamedDomainEvent(Id, previousTitle, title.Value, renamedAtUtc));

            return DomainResult.Success();
        }

        public DomainResult SetPriority(ContextPriority priority, DateTimeOffset changedAtUtc)
        {
            ArgumentNullException.ThrowIfNull(priority);

            if (Equals(Status, ArtifactStatusType.Archived))
            {
                return DomainResult.Failure(ArtifactErrors.ArchivedArtifactCannotChangePriority);
            }

            if (Priority == priority)
            {
                return DomainResult.Success();
            }

            int previousPriority = Priority.Value;
            Priority = priority;
            UpdatedAtUtc = changedAtUtc;

            RaiseDomainEvent(
                new ArtifactPriorityChangedDomainEvent(Id, previousPriority, priority.Value, changedAtUtc));

            return DomainResult.Success();
        }

        public DomainResult Archive(DateTimeOffset archivedAtUtc)
        {
            if (Equals(Status, ArtifactStatusType.Archived))
            {
                return DomainResult.Failure(ArtifactErrors.AlreadyArchived);
            }

            Status = ArtifactStatusType.Archived;
            ArchivedAtUtc = archivedAtUtc;
            UpdatedAtUtc = archivedAtUtc;

            RaiseDomainEvent(new ArtifactArchivedDomainEvent(Id, archivedAtUtc));

            return DomainResult.Success();
        }

        public DomainResult<Binding> CreateBinding(
            BindingId bindingId,
            ArtifactRevision revision,
            Workspace workspace,
            OrganizationId? organizationId,
            Project? project,
            string? repositoryCanonicalUri,
            string? repositoryRelativePathPrefix,
            string? branch,
            ProjectTask? task,
            string? agent,
            DateTimeOffset createdAtUtc)
        {
            ArgumentNullException.ThrowIfNull(revision);
            return Owns(revision)
                ? Binding.Create(bindingId, revision, workspace, organizationId, project, repositoryCanonicalUri,
                    repositoryRelativePathPrefix, branch, task, agent, createdAtUtc)
                : DomainResult<Binding>.Failure(BindingErrors.RevisionMismatch);
        }

        public DomainResult<InstructionRule> CreateInstructionRule(ArtifactRevision revision, RuleKey ruleKey,
            string? text, ContextPriority priority)
        {
            ArgumentNullException.ThrowIfNull(revision);
            if (!KindType.Equals(ArtifactKindType.Instruction))
            {
                return DomainResult<InstructionRule>.Failure(RuleErrors.InstructionKindRequired);
            }

            return Owns(revision)
                ? InstructionRule.Create(revision, ruleKey, text, priority)
                : DomainResult<InstructionRule>.Failure(RuleErrors.RevisionMismatch);
        }

        public DomainResult<PolicyRule> CreatePolicyRule(ArtifactRevision revision, RuleKey ruleKey, string? text,
            ContextPriority priority, PolicyEnforcementType enforcementType)
        {
            ArgumentNullException.ThrowIfNull(revision);
            if (!KindType.Equals(ArtifactKindType.Policy))
            {
                return DomainResult<PolicyRule>.Failure(RuleErrors.PolicyKindRequired);
            }

            return Owns(revision)
                ? PolicyRule.Create(revision, ruleKey, text, priority, enforcementType)
                : DomainResult<PolicyRule>.Failure(RuleErrors.RevisionMismatch);
        }

        public DomainResult<MemoryMetadata> CreateMemoryMetadata(
            MemoryId memoryId,
            ArtifactRevision revision,
            MemoryCategoryType categoryType,
            decimal confidence,
            bool userConfirmed,
            string? clientIdentity,
            string? sessionIdentity,
            DateTimeOffset capturedAtUtc,
            MemoryId? supersededMemoryId = null)
        {
            ArgumentNullException.ThrowIfNull(revision);
            if (!KindType.Equals(ArtifactKindType.Memory))
            {
                return DomainResult<MemoryMetadata>.Failure(MemoryErrors.MemoryKindRequired);
            }

            return Owns(revision)
                ? MemoryMetadata.Create(memoryId, revision, categoryType, confidence, userConfirmed, clientIdentity,
                    sessionIdentity, capturedAtUtc, supersededMemoryId)
                : DomainResult<MemoryMetadata>.Failure(MemoryErrors.RevisionMismatch);
        }

        private bool Owns(ArtifactRevision revision)
        {
            return revision.ArtifactId.Equals(Id)
                   && revision.WorkspaceId.Equals(WorkspaceId)
                   && revision.KindType.Equals(KindType);
        }

        public DomainResult<ArtifactRevision> CreateRevision(ArtifactRevisionId revisionId, ArtifactContent content,
            DateTimeOffset createdAtUtc)
        {
            ArgumentNullException.ThrowIfNull(revisionId);
            ArgumentNullException.ThrowIfNull(content);

            if (Status.Equals(ArtifactStatusType.Archived))
            {
                return DomainResult<ArtifactRevision>.Failure(ArtifactRevisionErrors.ArtifactArchived);
            }

            RevisionNumber nextNumber = CurrentRevisionNumber?.Next() ?? RevisionNumber.First();

            ArtifactRevision revision = ArtifactRevision.Create(revisionId, Id, WorkspaceId, KindType, nextNumber,
                content, createdAtUtc);

            CurrentRevisionId = revision.Id;
            CurrentRevisionNumber = revision.Number;
            UpdatedAtUtc = createdAtUtc;

            RaiseDomainEvent(new ArtifactRevisionCreatedDomainEvent(Id, revision.Id, revision.Number.Value,
                revision.ContentHash.Value, revision.SizeInBytes, createdAtUtc));

            return DomainResult<ArtifactRevision>.Success(revision);
        }
    }
}