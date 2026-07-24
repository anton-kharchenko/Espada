using Espada.Domain.Errors;

namespace Espada.Tests.Domain.ValueObjects;

public sealed class RevisionNumberTests
{
    [Fact]
    public void First_ShouldReturnOne()
    {
        // Act
        RevisionNumber number = RevisionNumber.First();

        // Assert
        number.Value.Should().Be(1);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(100)]
    public void Create_WithPositiveValue_ShouldCreateRevisionNumber(int value)
    {
        // Act
        RevisionNumber number = RevisionNumber.Create(value).ShouldSucceed();

        // Assert
        number.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithNonPositiveValue_ShouldReturnExpectedError(int value)
    {
        // Act
        DomainResult<RevisionNumber> result = RevisionNumber.Create(value);

        // Assert
        result.ShouldFailWith(ArtifactRevisionErrors.InvalidRevisionNumber);
    }

    [Fact]
    public void Next_ShouldReturnIncrementedNumber()
    {
        // Arrange
        RevisionNumber current = RevisionNumber.Create(5).ShouldSucceed();

        // Act
        RevisionNumber next = current.Next();

        // Assert
        current.Value.Should().Be(5);
        next.Value.Should().Be(6);
    }

    [Fact]
    public void Next_ShouldNotMutateOriginalNumber()
    {
        // Arrange
        RevisionNumber current = RevisionNumber.Create(5).ShouldSucceed();

        // Act
        _ = current.Next();

        // Assert
        current.Value.Should().Be(5);
    }

    [Fact]
    public void NumbersWithSameValue_ShouldBeEqual()
    {
        // Arrange
        RevisionNumber first = RevisionNumber.Create(3).ShouldSucceed();

        RevisionNumber second = RevisionNumber.Create(3).ShouldSucceed();

        // Assert
        first.Should().Be(second);
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void NumbersWithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        RevisionNumber first = RevisionNumber.Create(1).ShouldSucceed();

        RevisionNumber second = RevisionNumber.Create(2).ShouldSucceed();

        // Assert
        first.Should().NotBe(second);
    }
}