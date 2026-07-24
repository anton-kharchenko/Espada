using System.Text;
using Espada.Domain.Errors;

namespace Espada.Tests.Domain.ValueObjects;

public sealed class ArtifactContentTests
{
    [Theory]
    [MemberData(nameof(EmptyContentValues))]
    public void Create_WithEmptyContent_ShouldReturnExpectedError(string? value)
    {
        // Act
        DomainResult<ArtifactContent> result = ArtifactContent.Create(value);

        // Assert
        result.ShouldFailWith(ArtifactRevisionErrors.ContentEmpty);
    }

    [Fact]
    public void Create_WithValidContent_ShouldPreserveOriginalValue()
    {
        // Arrange
        const string value = "  # Heading\n\nContent with spaces.  ";

        // Act
        ArtifactContent content = ArtifactContent.Create(value).ShouldSucceed();

        // Assert
        content.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithValidContent_ShouldCalculateUtf8Size()
    {
        // Arrange
        const string value = "Привет, Espada!";

        // Act
        ArtifactContent content = ArtifactContent.Create(value).ShouldSucceed();

        // Assert
        content.SizeInBytes.Should().Be(Encoding.UTF8.GetByteCount(value));
    }

    [Fact]
    public void Create_WithValidContent_ShouldCalculateSha256Hash()
    {
        // Act
        ArtifactContent content = ArtifactContent.Create("hello").ShouldSucceed();

        // Assert
        content.Hash.Value.Should().Be("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824");
    }

    [Fact]
    public void EqualContent_ShouldProduceEqualHashes()
    {
        // Arrange
        ArtifactContent first = ArtifactContent.Create("Same content").ShouldSucceed();

        ArtifactContent second = ArtifactContent.Create("Same content").ShouldSucceed();

        // Assert
        first.Hash.Should().Be(second.Hash);
        first.Hash.Value.Should().Be(second.Hash.Value);
    }

    [Fact]
    public void DifferentContent_ShouldProduceDifferentHashes()
    {
        // Arrange
        ArtifactContent first = ArtifactContent.Create("First content").ShouldSucceed();

        ArtifactContent second = ArtifactContent.Create("Second content").ShouldSucceed();

        // Assert
        first.Hash.Should().NotBe(second.Hash);
        first.Hash.Value.Should().NotBe(second.Hash.Value);
    }

    [Fact]
    public void ContentDifferingOnlyByWhitespace_ShouldProduceDifferentHashes()
    {
        // Arrange
        ArtifactContent first = ArtifactContent.Create("Content").ShouldSucceed();

        ArtifactContent second = ArtifactContent.Create("Content ").ShouldSucceed();

        // Assert
        first.Hash.Should().NotBe(second.Hash);
    }

    [Fact]
    public void ContentWithSameValue_ShouldBeEqual()
    {
        // Arrange
        ArtifactContent first = ArtifactContent.Create("Same content").ShouldSucceed();

        ArtifactContent second = ArtifactContent.Create("Same content").ShouldSucceed();

        // Assert
        first.Should().Be(second);
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    public static TheoryData<string?> EmptyContentValues =>
    [
        null!,
        string.Empty,
        " ",
        "    ",
        "\t",
        "\r\n"
    ];
}