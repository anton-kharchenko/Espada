using Espada.Domain.Enums;
using Espada.Domain.Events;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Aggregates
{
    public sealed class Chunk : AggregateRoot<ChunkId>
    {
        private Chunk()
        {
        }

        private Chunk(
            ChunkId id,
            ChunkBatchId batchId,
            WorkspaceId workspaceId,
            ArtifactId artifactId,
            ArtifactRevisionId artifactRevisionId,
            ChunkNumber number,
            ChunkContent content,
            SourceTextSpan? sourceSpan,
            ChunkingStrategyType strategy,
            ChunkingVersion strategyVersion,
            DateTimeOffset createdAtUtc)
            : base(id)
        {
            BatchId = batchId;
            WorkspaceId = workspaceId;
            ArtifactId = artifactId;
            ArtifactRevisionId = artifactRevisionId;
            Number = number;
            Content = content;
            SourceSpan = sourceSpan;
            Strategy = strategy;
            StrategyVersion = strategyVersion;
            CreatedAtUtc = createdAtUtc;
        }

        public ChunkBatchId BatchId { get; } = null!;

        public WorkspaceId WorkspaceId { get; } = null!;

        public ArtifactId ArtifactId { get; } = null!;

        public ArtifactRevisionId ArtifactRevisionId { get; } = null!;

        public ChunkNumber Number { get; } = null!;

        public ChunkContent Content { get; } = null!;

        public SourceTextSpan? SourceSpan { get; }

        public ChunkingStrategyType Strategy { get; } = null!;

        public ChunkingVersion StrategyVersion { get; } = null!;

        public ContentHash ContentHash => Content.Hash;

        public int SizeInBytes => Content.SizeInBytes;

        public int CharacterCount => Content.CharacterCount;

        public DateTimeOffset CreatedAtUtc { get; private set; }

        public static DomainResult<Chunk> Create(
            ChunkId id,
            ChunkBatchId batchId,
            WorkspaceId workspaceId,
            ArtifactId artifactId,
            ArtifactRevisionId artifactRevisionId,
            ChunkNumber number,
            ChunkContent content,
            SourceTextSpan? sourceSpan,
            ChunkingStrategyType strategy,
            ChunkingVersion strategyVersion,
            DateTimeOffset createdAtUtc)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(batchId);
            ArgumentNullException.ThrowIfNull(workspaceId);
            ArgumentNullException.ThrowIfNull(artifactId);
            ArgumentNullException.ThrowIfNull(artifactRevisionId);
            ArgumentNullException.ThrowIfNull(number);
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(strategy);
            ArgumentNullException.ThrowIfNull(strategyVersion);

            Chunk chunk = new(id, batchId, workspaceId, artifactId, artifactRevisionId, number, content, sourceSpan,
                strategy, strategyVersion, createdAtUtc);

            chunk.RaiseDomainEvent(new ChunkCreatedDomainEvent(chunk.Id, chunk.BatchId, chunk.WorkspaceId,
                chunk.ArtifactId, chunk.ArtifactRevisionId, chunk.Number.Value, chunk.ContentHash.Value,
                chunk.SizeInBytes, chunk.SourceSpan?.Start, chunk.SourceSpan?.Length, chunk.Strategy,
                chunk.StrategyVersion.Value, createdAtUtc));

            return DomainResult<Chunk>.Success(chunk);
        }
    }
}