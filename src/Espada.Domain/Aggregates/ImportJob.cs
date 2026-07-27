using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Events;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates;

public sealed class ImportJob : AggregateRoot<ImportJobId>, IHasConcurrencyVersion
{
    public uint Version { get; private set; }

    private ImportJob()
    {
    }

    private ImportJob(
        ImportJobId id,
        SourceId sourceId,
        WorkspaceId workspaceId,
        DateTimeOffset requestedAtUtc)
        : base(id)
    {
        SourceId = sourceId;
        WorkspaceId = workspaceId;
        Status = ImportStatusType.Requested;
        RequestedAtUtc = requestedAtUtc;
    }

    public SourceId SourceId { get; private set; } = null!;

    public WorkspaceId WorkspaceId { get; private set; } = null!;

    public ImportStatusType Status { get; private set; } = null!;

    public DateTimeOffset RequestedAtUtc { get; private set; }

    public DateTimeOffset? StartedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public ArtifactId? ArtifactId { get; private set; }

    public ArtifactRevisionId? ArtifactRevisionId { get; private set; }

    public ImportFailure? Failure { get; private set; }

    public static DomainResult<ImportJob> Request(
        ImportJobId id,
        SourceId sourceId,
        WorkspaceId workspaceId,
        DateTimeOffset requestedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(sourceId);
        ArgumentNullException.ThrowIfNull(workspaceId);

        ImportJob importJob = new(id, sourceId, workspaceId, requestedAtUtc);

        importJob.RaiseDomainEvent(new ImportJobRequestedDomainEvent(importJob.Id, importJob.SourceId, importJob.WorkspaceId, requestedAtUtc));

        return DomainResult<ImportJob>.Success(importJob);
    }

    public DomainResult Start(DateTimeOffset startedAtUtc)
    {
        if (!Status.Equals(ImportStatusType.Requested))
        {
            return DomainResult.Failure(ImportJobErrors.CannotStart);
        }

        Status = ImportStatusType.Running;
        StartedAtUtc = startedAtUtc;

        RaiseDomainEvent(new ImportJobStartedDomainEvent(Id, startedAtUtc));

        return DomainResult.Success();
    }

    public DomainResult Complete(ArtifactId artifactId, ArtifactRevisionId artifactRevisionId, DateTimeOffset completedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(artifactId);
        ArgumentNullException.ThrowIfNull(artifactRevisionId);

        if (!Status.Equals(ImportStatusType.Running))
        {
            return DomainResult.Failure(ImportJobErrors.CannotComplete);
        }

        Status = ImportStatusType.Succeeded;
        ArtifactId = artifactId;
        ArtifactRevisionId = artifactRevisionId;
        CompletedAtUtc = completedAtUtc;

        RaiseDomainEvent(new ImportJobCompletedDomainEvent(Id, SourceId, artifactId, artifactRevisionId, completedAtUtc));

        return DomainResult.Success();
    }

    public DomainResult Fail(ImportFailure failure, DateTimeOffset failedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(failure);

        if (!Status.Equals(ImportStatusType.Running))
        {
            return DomainResult.Failure(ImportJobErrors.CannotFail);
        }

        Status = ImportStatusType.Failed;
        Failure = failure;
        CompletedAtUtc = failedAtUtc;

        RaiseDomainEvent(new ImportJobFailedDomainEvent(Id, failure.Code, failure.Reason, failedAtUtc));

        return DomainResult.Success();
    }

    public DomainResult Cancel(DateTimeOffset cancelledAtUtc)
    {
        bool canCancel = Status.Equals(ImportStatusType.Requested) || Status.Equals(ImportStatusType.Running);

        if (!canCancel)
        {
            return DomainResult.Failure(ImportJobErrors.CannotCancel);
        }

        Status = ImportStatusType.Cancelled;
        CompletedAtUtc = cancelledAtUtc;

        RaiseDomainEvent(new ImportJobCancelledDomainEvent(Id, cancelledAtUtc));

        return DomainResult.Success();
    }
}