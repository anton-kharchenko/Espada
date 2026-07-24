using System.Text;
using Espada.Domain.Errors;

namespace Espada.Tests.Domain.ValueObjects;

public sealed class ChunkContentTests
{
    [Theory]
    [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
    public void Create_WithEmptyValue_ShouldReturnExpectedError(string? value)
    {
        // Act
        DomainResult<ChunkContent> result = ChunkContent.Create(value);

        // Assert
        result.ShouldFailWith(ChunkErrors.ContentEmpty);
    }

    [Fact]
    public void Create_WithValidValue_ShouldPreserveOriginalFormatting()
    {
        // Arrange
        const string value = "  # Heading\n\nIndented content.  ";

        // Act
        ChunkContent content = ChunkContent.Create(value).ShouldSucceed();

        // Assert
        content.Value.Should().Be(value);
    }

    [Fact]
    public void Create_ShouldCalculateCharacterCount()
    {
        // Arrange
        const string value = "Chunk content";

        // Act
        ChunkContent content = ChunkContent.Create(value).ShouldSucceed();

        // Assert
        content.CharacterCount.Should().Be(value.Length);
    }

    [Fact]
    public void Create_ShouldCalculateUtf8Size()
    {
        // Arrange
        const string value = "Привет!";

        // Act
        ChunkContent content = ChunkContent.Create(value).ShouldSucceed();

        // Assert
        content.SizeInBytes.Should().Be(Encoding.UTF8.GetByteCount(value));
    }

    [Fact]
    public void Create_ShouldCalculateKnownSha256Hash()
    {
        // Act
        ChunkContent content = ChunkContent.Create("hello").ShouldSucceed();

        // Assert
        content.Hash.Value.Should().Be("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824");
    }

    [Fact]
    public void EqualContent_ShouldProduceEqualHashes()
    {
        // Arrange
        ChunkContent first = ChunkContent.Create("Same content").ShouldSucceed();

        ChunkContent second = ChunkContent.Create("Same content").ShouldSucceed();

        // Assert
        first.Hash.Should().Be(second.Hash);
        first.Should().Be(second);
    }

    [Fact]
    public void DifferentContent_ShouldProduceDifferentHashes()
    {
        // Arrange
        ChunkContent first = ChunkContent.Create("First content").ShouldSucceed();

        ChunkContent second = ChunkContent.Create("Second content").ShouldSucceed();

        // Assert
        first.Hash.Should().NotBe(second.Hash);
        first.Should().NotBe(second);
    }

    [Fact]
    public void WhitespaceDifference_ShouldChangeContentHash()
    {
        // Arrange
        ChunkContent first = ChunkContent.Create("Content").ShouldSucceed();

        ChunkContent second = ChunkContent.Create("Content ").ShouldSucceed();

        // Assert
        first.Hash.Should().NotBe(second.Hash);
    }
}