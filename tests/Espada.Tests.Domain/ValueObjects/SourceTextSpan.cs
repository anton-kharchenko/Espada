using Espada.Domain.Errors;

namespace Espada.Tests.Domain.ValueObjects;

public sealed class SourceTextSpanTests
{
    [Fact]
    public void Create_WithValidValues_ShouldCreateSpan()
    {
        // Act
        SourceTextSpan span = SourceTextSpan.Create(10, 25).ShouldSucceed();

        // Assert
        span.Start.Should().Be(10);
        span.Length.Should().Be(25);
        span.EndExclusive.Should().Be(35);
    }

    [Fact]
    public void Create_WithZeroStart_ShouldSucceed()
    {
        // Act
        SourceTextSpan span = SourceTextSpan.Create(0, 10).ShouldSucceed();

        // Assert
        span.Start.Should().Be(0);
        span.EndExclusive.Should().Be(10);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithNegativeStart_ShouldReturnExpectedError(int start)
    {
        // Act
        DomainResult<SourceTextSpan> result = SourceTextSpan.Create(start, 10);

        // Assert
        result.ShouldFailWith(ChunkErrors.SourceSpanStartInvalid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithNonPositiveLength_ShouldReturnExpectedError(int length)
    {
        // Act
        DomainResult<SourceTextSpan> result = SourceTextSpan.Create(0, length);

        // Assert
        result.ShouldFailWith(ChunkErrors.SourceSpanLengthInvalid);
    }

    [Fact]
    public void Create_WhenEndWouldOverflow_ShouldReturnExpectedError()
    {
        // Act
        DomainResult<SourceTextSpan> result = SourceTextSpan.Create(int.MaxValue, 1);

        // Assert
        result.ShouldFailWith(ChunkErrors.SourceSpanOverflow);
    }

    [Fact]
    public void Create_WhenEndEqualsMaximumInteger_ShouldSucceed()
    {
        // Act
        SourceTextSpan span = SourceTextSpan.Create(int.MaxValue - 10, 10).ShouldSucceed();

        // Assert
        span.EndExclusive.Should().Be(int.MaxValue);
    }

    [Fact]
    public void SpansWithSameValues_ShouldBeEqual()
    {
        // Arrange
        SourceTextSpan first = SourceTextSpan.Create(10, 20).ShouldSucceed();

        SourceTextSpan second = SourceTextSpan.Create(10, 20).ShouldSucceed();

        // Assert
        first.Should().Be(second);

        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void SpansWithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        SourceTextSpan first = SourceTextSpan.Create(10, 20).ShouldSucceed();

        SourceTextSpan second = SourceTextSpan.Create(11, 20).ShouldSucceed();

        // Assert
        first.Should().NotBe(second);
    }

    [Fact]
    public void ToString_ShouldUseHalfOpenRangeNotation()
    {
        // Arrange
        SourceTextSpan span = SourceTextSpan.Create(10, 20).ShouldSucceed();

        // Act
        string value = span.ToString();

        // Assert
        value.Should().Be("[10..30)");
    }
}