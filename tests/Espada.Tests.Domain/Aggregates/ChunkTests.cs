using Espada.Tests.Domain.TestData.Builders;
using System.Text;

namespace Espada.Tests.Domain.Aggregates;

public sealed class ChunkTests
{
    [Fact]
    public void Create_WithValidArguments_ShouldCreateChunk()
    {
        // Act
        Chunk chunk = new ChunkBuilder()
            .WithId(TestIds.DefaultChunkId)
            .InBatch(TestIds.DefaultChunkBatchId)
            .InWorkspace(TestIds.DefaultWorkspaceId)
            .ForArtifact(TestIds.DefaultArtifactId)
            .ForRevision(TestIds.FirstRevisionId)
            .WithNumber(3)
            .WithContent("Chunk content.")
            .WithSourceSpan(20, 14)
            .WithStrategy(ChunkingStrategyType.Recursive, "recursive-v1")
            .CreatedAt(TestDates.ChunkCreatedAtUtc)
            .Build();

        // Assert
        chunk.Id.Should().Be(TestIds.DefaultChunkId);
        chunk.BatchId.Should().Be(TestIds.DefaultChunkBatchId);
        chunk.WorkspaceId.Should().Be(TestIds.DefaultWorkspaceId);
        chunk.ArtifactId.Should().Be(TestIds.DefaultArtifactId);
        chunk.ArtifactRevisionId.Should().Be(TestIds.FirstRevisionId);

        chunk.Number.Value.Should().Be(3);
        chunk.Content.Value.Should().Be("Chunk content.");

        chunk.SourceSpan.Should().NotBeNull();
        chunk.SourceSpan!.Start.Should().Be(20);
        chunk.SourceSpan.Length.Should().Be(14);
        chunk.SourceSpan.EndExclusive.Should().Be(34);

        chunk.Strategy.Should().Be(ChunkingStrategyType.Recursive);

        chunk.StrategyVersion.Value.Should().Be("recursive-v1");

        chunk.CreatedAtUtc.Should().Be(TestDates.ChunkCreatedAtUtc);
    }

    [Fact]
    public void Create_ShouldCalculateContentMetadata()
    {
        // Arrange
        const string value = "Привет, Espada!";

        // Act
        Chunk chunk = new ChunkBuilder()
            .WithContent(value)
            .Build();

        // Assert
        chunk.CharacterCount.Should().Be(value.Length);

        chunk.SizeInBytes.Should().Be(Encoding.UTF8.GetByteCount(value));

        chunk.ContentHash.Should().Be(chunk.Content.Hash);

        chunk.ContentHash.Value
            .Should()
            .MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void Create_WithSourceSpan_ShouldPreserveSourcePosition()
    {
        // Act
        Chunk chunk = new ChunkBuilder()
            .WithSourceSpan(100, 50)
            .Build();

        // Assert
        chunk.SourceSpan.Should().NotBeNull();
        chunk.SourceSpan!.Start.Should().Be(100);
        chunk.SourceSpan.Length.Should().Be(50);
        chunk.SourceSpan.EndExclusive.Should().Be(150);
    }

    [Fact]
    public void Create_WithoutSourceSpan_ShouldAllowNullSourceSpan()
    {
        // Act
        Chunk chunk = new ChunkBuilder()
            .WithoutSourceSpan()
            .Build();

        // Assert
        chunk.SourceSpan.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldPreserveChunkingStrategyInformation()
    {
        // Act
        Chunk chunk = new ChunkBuilder()
            .WithStrategy(ChunkingStrategyType.Markdown, "markdown-v2")
            .Build();

        // Assert
        chunk.Strategy.Should().Be(ChunkingStrategyType.Markdown);

        chunk.StrategyVersion.Value.Should().Be("markdown-v2");
    }

    [Fact]
    public void Create_WithValidArguments_ShouldRaiseCreatedEvent()
    {
        // Arrange
        const string content = "Chunk event content.";

        // Act
        Chunk chunk = new ChunkBuilder()
            .WithNumber(2)
            .WithContent(content)
            .WithSourceSpan(40, content.Length)
            .WithStrategy(ChunkingStrategyType.Markdown, "markdown-v1")
            .Build();

        // Assert
        ChunkCreatedDomainEvent domainEvent = chunk.ShouldHaveSingleDomainEvent<ChunkCreatedDomainEvent>();

        domainEvent.ChunkId.Should().Be(TestIds.DefaultChunkId);
        domainEvent.BatchId.Should().Be(TestIds.DefaultChunkBatchId);
        domainEvent.WorkspaceId.Should().Be(TestIds.DefaultWorkspaceId);
        domainEvent.ArtifactId.Should().Be(TestIds.DefaultArtifactId);
        domainEvent.ArtifactRevisionId.Should().Be(TestIds.FirstRevisionId);
        domainEvent.ChunkNumber.Should().Be(2);
        domainEvent.ContentHash.Should().Be(chunk.ContentHash.Value);
        domainEvent.SizeInBytes.Should().Be(chunk.SizeInBytes);
        domainEvent.SourceStart.Should().Be(40);
        domainEvent.SourceLength.Should().Be(content.Length);
        domainEvent.Strategy.Should().Be(ChunkingStrategyType.Markdown);
        domainEvent.StrategyVersion.Should().Be("markdown-v1");
        domainEvent.CreatedAtUtc.Should().Be(TestDates.ChunkCreatedAtUtc);
    }

    [Fact]
    public void Create_WithoutSourceSpan_ShouldRaiseEventWithNullSpan()
    {
        // Act
        Chunk chunk = new ChunkBuilder()
            .WithoutSourceSpan()
            .Build();

        // Assert
        ChunkCreatedDomainEvent domainEvent = chunk.ShouldHaveSingleDomainEvent<ChunkCreatedDomainEvent>();

        domainEvent.SourceStart.Should().BeNull();
        domainEvent.SourceLength.Should().BeNull();
    }

    [Fact]
    public void ChunksWithSameContent_ShouldHaveSameContentHash()
    {
        // Arrange
        const string content = "Identical content.";

        // Act
        Chunk first = new ChunkBuilder()
            .WithId(TestIds.DefaultChunkId)
            .WithNumber(1)
            .WithContent(content)
            .Build();

        Chunk second = new ChunkBuilder()
            .WithId(TestIds.SecondChunkId)
            .WithNumber(2)
            .WithContent(content)
            .Build();

        // Assert
        first.Id.Should().NotBe(second.Id);

        first.ContentHash.Should().Be(second.ContentHash);
    }
}