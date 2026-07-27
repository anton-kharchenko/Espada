using Espada.Domain.Errors;

namespace Espada.Tests.Domain.ValueObjects;

public sealed class ChunkingVersionTests
{
    public static TheoryData<string?> EmptyValues =>
        new()
        {
            null!,
            string.Empty,
            " ",
            "\t"
        };

    [Fact]
    public void Create_WithValidValue_ShouldCreateVersion()
    {
        // Act
        ChunkingVersion version = ChunkingVersion.Create("recursive-v1").ShouldSucceed();

        // Assert
        version.Value.Should().Be("recursive-v1");
    }

    [Fact]
    public void Create_ShouldTrimValue()
    {
        // Act
        ChunkingVersion version = ChunkingVersion.Create("  markdown-v2  ").ShouldSucceed();

        // Assert
        version.Value.Should().Be("markdown-v2");
    }

    [Theory]
    [MemberData(nameof(EmptyValues))]
    public void Create_WithEmptyValue_ShouldReturnExpectedError(string? value)
    {
        // Act
        DomainResult<ChunkingVersion> result = ChunkingVersion.Create(value);

        // Assert
        result.ShouldFailWith(ChunkErrors.VersionEmpty);
    }

    [Fact]
    public void Create_WithMaximumLength_ShouldSucceed()
    {
        // Arrange
        string value = new('a', ChunkingVersion.MaxLength);

        // Act
        ChunkingVersion version = ChunkingVersion.Create(value).ShouldSucceed();

        // Assert
        version.Value.Should().HaveLength(ChunkingVersion.MaxLength);
    }

    [Fact]
    public void Create_AboveMaximumLength_ShouldReturnExpectedError()
    {
        // Arrange
        string value = new('a', ChunkingVersion.MaxLength + 1);

        // Act
        DomainResult<ChunkingVersion> result = ChunkingVersion.Create(value);

        // Assert
        result.ShouldFailWith(ChunkErrors.VersionTooLong);
    }

    [Fact]
    public void VersionsWithSameValue_ShouldBeEqual()
    {
        // Arrange
        ChunkingVersion first = ChunkingVersion.Create("recursive-v1").ShouldSucceed();

        ChunkingVersion second = ChunkingVersion.Create("recursive-v1").ShouldSucceed();

        // Assert
        first.Should().Be(second);
    }

    [Fact]
    public void VersionComparison_ShouldRemainCaseSensitive()
    {
        // Arrange
        ChunkingVersion first = ChunkingVersion.Create("recursive-v1").ShouldSucceed();

        ChunkingVersion second = ChunkingVersion.Create("Recursive-v1").ShouldSucceed();

        // Assert
        first.Should().NotBe(second);
    }
}