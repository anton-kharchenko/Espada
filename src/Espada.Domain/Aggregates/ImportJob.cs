using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Events;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class ImportJob : AggregateRoot<ImportJobId>, IHasConcurrencyVersion
    {
        private ImportJob()
        {
        }

        private ImportJob(
            ImportJobId id,
            SourceId sourceId,
            WorkspaceId workspaceId,
            DateTimeOffset requestedAtUtc,
            string idempotencyKey,
            string requestFingerprint,
            string optionsJson)
            : base(id)
        {
            SourceId = sourceId;
            WorkspaceId = workspaceId;
            Status = ImportStatusType.Requested;
            Stage = ImportPipelineStageType.Start;
            IdempotencyKey = idempotencyKey;
            RequestFingerprint = requestFingerprint;
            OptionsJson = optionsJson;
            RequestedAtUtc = requestedAtUtc;
        }

        public SourceId SourceId { get; } = null!;

        public WorkspaceId WorkspaceId { get; } = null!;

        public ImportStatusType Status { get; private set; } = null!;

        public ImportPipelineStageType Stage { get; private set; } = null!;

        public string IdempotencyKey { get; private set; } = string.Empty;

        public string RequestFingerprint { get; private set; } = string.Empty;

        public string OptionsJson { get; private set; } = "{}";

        public DateTimeOffset RequestedAtUtc { get; private set; }

        public DateTimeOffset? StartedAtUtc { get; private set; }

        public DateTimeOffset? CompletedAtUtc { get; private set; }

        public ArtifactId? ArtifactId { get; private set; }

        public ArtifactRevisionId? ArtifactRevisionId { get; private set; }

        public ChunkBatchId? ChunkBatchId { get; private set; }

        public string? RawBlobHash { get; private set; }

        public string? ParsedBlobHash { get; private set; }

        public ImportFailure? Failure { get; private set; }
        public uint Version { get; private set; }

        public static DomainResult<ImportJob> Request(
            ImportJobId id,
            SourceId sourceId,
            WorkspaceId workspaceId,
            DateTimeOffset requestedAtUtc,
            string? idempotencyKey = null,
            string? requestFingerprint = null,
            string optionsJson = "{}")
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(sourceId);
            ArgumentNullException.ThrowIfNull(workspaceId);

            string resolvedIdempotencyKey = idempotencyKey ?? id.Value.ToString("N");
            string resolvedFingerprint = requestFingerprint ?? id.Value.ToString("N");
            ArgumentException.ThrowIfNullOrWhiteSpace(resolvedIdempotencyKey);
            ArgumentException.ThrowIfNullOrWhiteSpace(resolvedFingerprint);
            ArgumentException.ThrowIfNullOrWhiteSpace(optionsJson);

            ImportJob importJob = new(
                id,
                sourceId,
                workspaceId,
                requestedAtUtc,
                resolvedIdempotencyKey,
                resolvedFingerprint,
                optionsJson);

            importJob.RaiseDomainEvent(new ImportJobRequestedDomainEvent(importJob.Id, importJob.SourceId,
                importJob.WorkspaceId, requestedAtUtc));

            return DomainResult<ImportJob>.Success(importJob);
        }

        public DomainResult CompleteStage(ImportPipelineStageType completedStage, DateTimeOffset completedAtUtc)
        {
            if (completedStage.Id < Stage.Id)
            {
                return DomainResult.Success();
            }

            if (!completedStage.Equals(Stage) || Status.Equals(ImportStatusType.Succeeded) ||
                Status.Equals(ImportStatusType.Failed) || Status.Equals(ImportStatusType.Cancelled))
            {
                return DomainResult.Failure(ImportJobErrors.CannotAdvanceStage);
            }

            if (completedStage.Equals(ImportPipelineStageType.Complete))
            {
                return DomainResult.Failure(ImportJobErrors.CannotAdvanceStage);
            }

            if (completedStage.Equals(ImportPipelineStageType.Start))
            {
                Status = ImportStatusType.Running;
                StartedAtUtc = completedAtUtc;
            }

            Stage = Enumeration.GetAll<ImportPipelineStageType>().Single(stage => stage.Id == completedStage.Id + 1);
            RaiseDomainEvent(new ImportStageScheduledDomainEvent(Id, Stage, completedAtUtc));

            return DomainResult.Success();
        }

        public DomainResult RecordRawSnapshot(string blobHash)
        {
            return RecordReference(RawBlobHash, blobHash, value => RawBlobHash = value);
        }

        public DomainResult RecordParsedSnapshot(string blobHash)
        {
            return RecordReference(ParsedBlobHash, blobHash, value => ParsedBlobHash = value);
        }

        public DomainResult RecordMaterializedArtifact(ArtifactId artifactId, ArtifactRevisionId revisionId)
        {
            ArgumentNullException.ThrowIfNull(artifactId);
            ArgumentNullException.ThrowIfNull(revisionId);

            if ((ArtifactId is not null && ArtifactId != artifactId)
                || (ArtifactRevisionId is not null && ArtifactRevisionId != revisionId))
            {
                return DomainResult.Failure(ImportJobErrors.PipelineReferenceConflict);
            }

            ArtifactId = artifactId;
            ArtifactRevisionId = revisionId;
            return DomainResult.Success();
        }

        public DomainResult RecordChunkBatch(ChunkBatchId chunkBatchId)
        {
            ArgumentNullException.ThrowIfNull(chunkBatchId);
            if (ChunkBatchId is not null && ChunkBatchId != chunkBatchId)
            {
                return DomainResult.Failure(ImportJobErrors.PipelineReferenceConflict);
            }

            ChunkBatchId = chunkBatchId;
            return DomainResult.Success();
        }

        private static DomainResult RecordReference(
            string? existing,
            string value,
            Action<string> assign)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            if (existing is not null && !string.Equals(existing, value, StringComparison.Ordinal))
            {
                return DomainResult.Failure(ImportJobErrors.PipelineReferenceConflict);
            }

            assign(value);
            return DomainResult.Success();
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

        public DomainResult Complete(ArtifactId artifactId, ArtifactRevisionId artifactRevisionId,
            DateTimeOffset completedAtUtc)
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

            RaiseDomainEvent(new ImportJobCompletedDomainEvent(Id, SourceId, artifactId, artifactRevisionId,
                completedAtUtc));

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
}