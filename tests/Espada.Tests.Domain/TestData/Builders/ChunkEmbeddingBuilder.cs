namespace Espada.Tests.Domain.TestData.Builders
{
    internal sealed class ChunkEmbeddingBuilder
    {
        private ContentHash _chunkContentHash = ContentHash.FromUtf8("Default chunk content.");

        private ChunkId _chunkId = TestIds.DefaultChunkId;

        private DateTimeOffset _createdAtUtc = TestDates.ChunkEmbeddingCreatedAtUtc;

        private EmbeddingDimensions _dimensions = EmbeddingDimensions.Create(1536).ShouldSucceed();
        private ChunkEmbeddingId _id = TestIds.DefaultChunkEmbeddingId;

        private EmbeddingModel _model = EmbeddingModel.Create("openai/text-embedding-3-small", "2026-01")
            .ShouldSucceed();

        private WorkspaceId _workspaceId = TestIds.DefaultWorkspaceId;

        public ChunkEmbeddingBuilder WithId(ChunkEmbeddingId id)
        {
            _id = id;
            return this;
        }

        public ChunkEmbeddingBuilder InWorkspace(WorkspaceId workspaceId)
        {
            _workspaceId = workspaceId;
            return this;
        }

        public ChunkEmbeddingBuilder ForChunk(ChunkId chunkId)
        {
            _chunkId = chunkId;
            return this;
        }

        public ChunkEmbeddingBuilder WithContentHash(ContentHash contentHash)
        {
            _chunkContentHash = contentHash;
            return this;
        }

        public ChunkEmbeddingBuilder WithContentHashFor(string content)
        {
            _chunkContentHash = ContentHash.FromUtf8(content);

            return this;
        }

        public ChunkEmbeddingBuilder WithModel(string identifier, string version)
        {
            _model = EmbeddingModel
                .Create(identifier, version)
                .ShouldSucceed();

            return this;
        }

        public ChunkEmbeddingBuilder WithDimensions(int dimensions)
        {
            _dimensions = EmbeddingDimensions
                .Create(dimensions)
                .ShouldSucceed();

            return this;
        }

        public ChunkEmbeddingBuilder CreatedAt(DateTimeOffset createdAtUtc)
        {
            _createdAtUtc = createdAtUtc;
            return this;
        }

        public ChunkEmbedding Build()
        {
            return ChunkEmbedding
                .Create(_id, _workspaceId, _chunkId, _chunkContentHash, _model, _dimensions, _createdAtUtc)
                .ShouldSucceed();
        }
    }
}