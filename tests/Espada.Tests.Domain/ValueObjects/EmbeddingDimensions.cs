using Espada.Domain.Errors;

namespace Espada.Tests.Domain.ValueObjects;

public sealed class EmbeddingDimensionsTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(384)]
    [InlineData(768)]
    [InlineData(1536)]
    [InlineData(3072)]
    public void Create_WithPositiveValue_ShouldCreateDimensions(int value)
    {
        // Act
        EmbeddingDimensions dimensions = EmbeddingDimensions.Create(value).ShouldSucceed();

        // Assert
        dimensions.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-1536)]
    public void Create_WithNonPositiveValue_ShouldReturnExpectedError(int value)
    {
        // Act
        DomainResult<EmbeddingDimensions> result = EmbeddingDimensions.Create(value);

        // Assert
        result.ShouldFailWith(ChunkEmbeddingErrors.DimensionsInvalid);
    }

    [Fact]
    public void DimensionsWithSameValue_ShouldBeEqual()
    {
        // Arrange
        EmbeddingDimensions first = EmbeddingDimensions.Create(1536).ShouldSucceed();

        EmbeddingDimensions second = EmbeddingDimensions.Create(1536).ShouldSucceed();

        // Assert
        first.Should().Be(second);

        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void DimensionsWithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        EmbeddingDimensions first = EmbeddingDimensions.Create(1536).ShouldSucceed();

        EmbeddingDimensions second = EmbeddingDimensions.Create(3072).ShouldSucceed();

        // Assert
        first.Should().NotBe(second);
    }

    [Fact]
    public void ToString_ShouldReturnNumericValue()
    {
        // Arrange
        EmbeddingDimensions dimensions = EmbeddingDimensions.Create(1536).ShouldSucceed();

        // Act
        string value = dimensions.ToString();

        // Assert
        value.Should().Be("1536");
    }
}