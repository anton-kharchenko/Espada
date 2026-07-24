using Espada.Tests.Domain.TestData.Builders;

namespace Espada.Tests.Domain.Aggregates;

public sealed class ChunkEmbeddingTests
{
    [Fact]
    public void Create_WithValidArguments_ShouldCreateChunkEmbedding()
    {
        // Arrange
        ContentHash contentHash = ContentHash.FromUtf8("Embedding source content.");

        // Act
        ChunkEmbedding embedding = new ChunkEmbeddingBuilder()
                .WithId(TestIds.DefaultChunkEmbeddingId)
                .InWorkspace(TestIds.DefaultWorkspaceId)
                .ForChunk(TestIds.DefaultChunkId)
                .WithContentHash(contentHash)
                .WithModel("openai/text-embedding-3-small", "2026-01")
                .WithDimensions(1536)
                .CreatedAt(TestDates.ChunkEmbeddingCreatedAtUtc)
                .Build();

        // Assert
        embedding.Id.Should().Be(TestIds.DefaultChunkEmbeddingId);
        embedding.WorkspaceId.Should().Be(TestIds.DefaultWorkspaceId);
        embedding.ChunkId.Should().Be(TestIds.DefaultChunkId);

        embedding.ChunkContentHash.Should().Be(contentHash);
        embedding.Model.Identifier.Should().Be("openai/text-embedding-3-small");
        embedding.Model.Version.Should().Be("2026-01");

        embedding.Dimensions.Value.Should().Be(1536);
        embedding.CreatedAtUtc.Should().Be(TestDates.ChunkEmbeddingCreatedAtUtc);
    }

    [Fact]
    public void Create_ShouldPreserveChunkContentHash()
    {
        // Arrange
        const string chunkContent = "Exact chunk content.";

        ContentHash expectedHash = ContentHash.FromUtf8(chunkContent);

        // Act
        ChunkEmbedding embedding = new ChunkEmbeddingBuilder()
                .WithContentHashFor(chunkContent)
                .Build();

        // Assert
        embedding.ChunkContentHash.Should().Be(expectedHash);
        embedding.ChunkContentHash.Value.Should().Be(expectedHash.Value);
        embedding.ChunkContentHash.Value.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void Create_ShouldPreserveEmbeddingModel()
    {
        // Act
        ChunkEmbedding embedding = new ChunkEmbeddingBuilder()
                .WithModel("sentence-transformers/multilingual-e5-large", "sha256:abc123")
                .Build();

        // Assert
        embedding.Model.Identifier.Should().Be("sentence-transformers/multilingual-e5-large");
        embedding.Model.Version.Should().Be("sha256:abc123");
        embedding.Model.ToString().Should().Be("sentence-transformers/multilingual-e5-large" + "@sha256:abc123");
    }

    [Fact]
    public void Create_ShouldPreserveDimensions()
    {
        // Act
        ChunkEmbedding embedding = new ChunkEmbeddingBuilder().WithDimensions(3072).Build();

        // Assert
        embedding.Dimensions.Value.Should().Be(3072);
    }

    [Fact]
    public void Create_WithValidArguments_ShouldRaiseCreatedEvent()
    {
        // Arrange
        const string chunkContent = "Content used for an embedding.";

        ContentHash contentHash = ContentHash.FromUtf8(chunkContent);

        // Act
        ChunkEmbedding embedding = new ChunkEmbeddingBuilder()
                .WithContentHash(contentHash)
                .WithModel("openai/text-embedding-3-small", "2026-01")
                .WithDimensions(1536)
                .Build();

        // Assert
        ChunkEmbeddingCreatedDomainEvent domainEvent = embedding.ShouldHaveSingleDomainEvent<ChunkEmbeddingCreatedDomainEvent>();

        domainEvent.ChunkEmbeddingId.Should().Be(TestIds.DefaultChunkEmbeddingId);
        domainEvent.WorkspaceId.Should().Be(TestIds.DefaultWorkspaceId);
        domainEvent.ChunkId.Should().Be(TestIds.DefaultChunkId);

        domainEvent.ChunkContentHash.Should().Be(contentHash.Value);
        domainEvent.ModelIdentifier.Should().Be("openai/text-embedding-3-small");
        domainEvent.ModelVersion.Should().Be("2026-01");
        domainEvent.Dimensions.Should().Be(1536);
        domainEvent.CreatedAtUtc.Should().Be(TestDates.ChunkEmbeddingCreatedAtUtc);
    }

    [Fact]
    public void EmbeddingsForSameChunkWithDifferentModels_ShouldBeDistinct()
    {
        // Arrange
        ContentHash contentHash = ContentHash.FromUtf8("Same chunk content.");

        // Act
        ChunkEmbedding first = new ChunkEmbeddingBuilder()
                .WithId(TestIds.DefaultChunkEmbeddingId)
                .WithContentHash(contentHash)
                .WithModel("openai/text-embedding-3-small", "2026-01")
                .WithDimensions(1536)
                .Build();

        ChunkEmbedding second = new ChunkEmbeddingBuilder()
                .WithId(TestIds.SecondChunkEmbeddingId)
                .WithContentHash(contentHash)
                .WithModel("openai/text-embedding-3-large", "2026-01")
                .WithDimensions(3072)
                .Build();

        // Assert
        first.Id.Should().NotBe(second.Id);
        first.ChunkId.Should().Be(second.ChunkId);

        first.ChunkContentHash.Should().Be(second.ChunkContentHash);
        first.Model.Should().NotBe(second.Model);
        first.Dimensions.Should().NotBe(second.Dimensions);
    }

    [Fact]
    public void EmbeddingsForDifferentContent_ShouldHaveDifferentHashes()
    {
        // Act
        ChunkEmbedding first = new ChunkEmbeddingBuilder()
                .WithId(TestIds.DefaultChunkEmbeddingId)
                .WithContentHashFor("First chunk content.")
                .Build();

        ChunkEmbedding second = new ChunkEmbeddingBuilder()
                .WithId(TestIds.SecondChunkEmbeddingId)
                .WithContentHashFor("Second chunk content.")
                .Build();

        // Assert
        first.ChunkContentHash.Should().NotBe(second.ChunkContentHash);
    }
}