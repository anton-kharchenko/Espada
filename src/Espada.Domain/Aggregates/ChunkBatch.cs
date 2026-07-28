using Espada.Domain.Enums;
using Espada.Domain.Errors;
using Espada.Domain.Events;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class ChunkBatch : AggregateRoot<ChunkBatchId>, IHasConcurrencyVersion
    {
        private ChunkBatch() { }

        private ChunkBatch(
            ChunkBatchId id,
            WorkspaceId workspaceId,
            ArtifactId artifactId,
            ArtifactRevisionId artifactRevisionId,
            ChunkingStrategyType strategy,
            ChunkingVersion strategyVersion,
            DateTimeOffset requestedAtUtc)
            : base(id)
        {
            WorkspaceId = workspaceId;
            ArtifactId = artifactId;
            ArtifactRevisionId = artifactRevisionId;
            Strategy = strategy;
            StrategyVersion = strategyVersion;
            Status = ChunkBatchStatusType.Requested;
            RequestedAtUtc = requestedAtUtc;
        }

        public WorkspaceId WorkspaceId { get; } = null!;
        public ArtifactId ArtifactId { get; } = null!;
        public ArtifactRevisionId ArtifactRevisionId { get; } = null!;
        public ChunkingStrategyType Strategy { get; } = null!;
        public ChunkingVersion StrategyVersion { get; } = null!;
        public ChunkBatchStatusType Status { get; private set; } = null!;
        public DateTimeOffset RequestedAtUtc { get; private set; }
        public DateTimeOffset? StartedAtUtc { get; private set; }
        public DateTimeOffset? CompletedAtUtc { get; private set; }
        public int? ChunkCount { get; private set; }
        public string? FailureReason { get; private set; }
        public uint Version { get; private set; }

        public static DomainResult<ChunkBatch> Request(
            ChunkBatchId id,
            WorkspaceId workspaceId,
            ArtifactId artifactId,
            ArtifactRevisionId artifactRevisionId,
            ChunkingStrategyType strategy,
            ChunkingVersion strategyVersion,
            DateTimeOffset requestedAtUtc)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(workspaceId);
            ArgumentNullException.ThrowIfNull(artifactId);
            ArgumentNullException.ThrowIfNull(artifactRevisionId);
            ArgumentNullException.ThrowIfNull(strategy);
            ArgumentNullException.ThrowIfNull(strategyVersion);

            ChunkBatch batch = new(id, workspaceId, artifactId, artifactRevisionId, strategy, strategyVersion,
                requestedAtUtc);
            batch.RaiseDomainEvent(new ChunkBatchRequestedDomainEvent(batch.Id, batch.WorkspaceId, batch.ArtifactId,
                batch.ArtifactRevisionId, batch.Strategy, batch.StrategyVersion.Value, requestedAtUtc));
            return DomainResult<ChunkBatch>.Success(batch);
        }

        public DomainResult Start(DateTimeOffset startedAtUtc)
        {
            if (!Status.Equals(ChunkBatchStatusType.Requested))
            {
                return DomainResult.Failure(ChunkBatchErrors.CannotStart);
            }

            Status = ChunkBatchStatusType.Running;
            StartedAtUtc = startedAtUtc;
            RaiseDomainEvent(new ChunkBatchStartedDomainEvent(Id, startedAtUtc));
            return DomainResult.Success();
        }

        public DomainResult Complete(int chunkCount, DateTimeOffset completedAtUtc)
        {
            if (!Status.Equals(ChunkBatchStatusType.Running))
            {
                return DomainResult.Failure(ChunkBatchErrors.CannotComplete);
            }

            if (chunkCount < 1)
            {
                return DomainResult.Failure(ChunkBatchErrors.ChunkCountInvalid);
            }

            Status = ChunkBatchStatusType.Succeeded;
            ChunkCount = chunkCount;
            CompletedAtUtc = completedAtUtc;
            RaiseDomainEvent(new ChunkBatchCompletedDomainEvent(Id, chunkCount, completedAtUtc));
            return DomainResult.Success();
        }

        public DomainResult Fail(string? reason, DateTimeOffset failedAtUtc)
        {
            if (!Status.Equals(ChunkBatchStatusType.Running))
            {
                return DomainResult.Failure(ChunkBatchErrors.CannotFail);
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return DomainResult.Failure(ChunkBatchErrors.FailureReasonEmpty);
            }

            Status = ChunkBatchStatusType.Failed;
            FailureReason = reason.Trim();
            CompletedAtUtc = failedAtUtc;
            RaiseDomainEvent(new ChunkBatchFailedDomainEvent(Id, FailureReason, failedAtUtc));
            return DomainResult.Success();
        }
    }
}