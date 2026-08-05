namespace Espada.Tests.Domain.TestData.Builders
{
    internal sealed class ChunkBuilder
    {
        private ArtifactId _artifactId = TestIds.DefaultArtifactId;

        private ArtifactRevisionId _artifactRevisionId = TestIds.FirstRevisionId;

        private ChunkBatchId _batchId = TestIds.DefaultChunkBatchId;

        private ChunkContent _content = ChunkContent.Create("Default chunk content.").ShouldSucceed();

        private DateTimeOffset _createdAtUtc = TestDates.ChunkCreatedAtUtc;
        private ChunkId _id = TestIds.DefaultChunkId;

        private ChunkNumber _number = ChunkNumber.First();

        private SourceTextSpan? _sourceSpan = SourceTextSpan.Create(0, 22).ShouldSucceed();

        private ChunkingStrategyType _strategy = ChunkingStrategyType.Recursive;

        private ChunkingVersion _strategyVersion = ChunkingVersion.Create("recursive-v1").ShouldSucceed();

        private WorkspaceId _workspaceId = TestIds.DefaultWorkspaceId;

        public ChunkBuilder WithId(ChunkId id)
        {
            _id = id;
            return this;
        }

        public ChunkBuilder InBatch(ChunkBatchId batchId)
        {
            _batchId = batchId;
            return this;
        }

        public ChunkBuilder InWorkspace(WorkspaceId workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public ChunkBuilder ForArtifact(ArtifactId artifactId)
        {
            _artifactId = artifactId;
            return this;
        }

        public ChunkBuilder ForRevision(ArtifactRevisionId artifactRevisionId)
        {
            _artifactRevisionId = artifactRevisionId;
            return this;
        }

        public ChunkBuilder WithNumber(int number)
        {
            _number = ChunkNumber.Create(number)
                .ShouldSucceed();

            return this;
        }

        public ChunkBuilder WithContent(string content)
        {
            _content = ChunkContent.Create(content).ShouldSucceed();

            return this;
        }

        public ChunkBuilder WithSourceSpan(int start, int length)
        {
            _sourceSpan = SourceTextSpan.Create(start, length).ShouldSucceed();

            return this;
        }

        public ChunkBuilder WithoutSourceSpan()
        {
            _sourceSpan = null;
            return this;
        }

        public ChunkBuilder WithStrategy(ChunkingStrategyType strategy, string version)
        {
            _strategy = strategy;

            _strategyVersion = ChunkingVersion.Create(version).ShouldSucceed();

            return this;
        }

        public ChunkBuilder CreatedAt(DateTimeOffset createdAtUtc)
        {
            _createdAtUtc = createdAtUtc;
            return this;
        }

        public Chunk Build()
        {
            return Chunk
                .Create(_id, _batchId, _workspaceId, _artifactId, _artifactRevisionId, _number, _content, _sourceSpan,
                    _strategy, _strategyVersion, _createdAtUtc)
                .ShouldSucceed();
        }
    }
}