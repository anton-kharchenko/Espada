using Espada.Domain.Errors;

namespace Espada.Tests.Domain.ValueObjects
{
    public sealed class ChunkNumberTests
    {
        public static TheoryData<int> PositiveValues => new() { 1, 2, 100 };

        public static TheoryData<int> NonPositiveValues => new() { 0, -1, -100 };

        [Fact]
        public void First_ShouldReturnOne()
        {
            // Act
            ChunkNumber number = ChunkNumber.First();

            // Assert
            number.Value.Should().Be(1);
        }

        [Theory]
        [MemberData(nameof(PositiveValues))]
        public void Create_WithPositiveValue_ShouldCreateNumber(int value)
        {
            // Act
            ChunkNumber number = ChunkNumber.Create(value).ShouldSucceed();

            // Assert
            number.Value.Should().Be(value);
        }

        [Theory]
        [MemberData(nameof(NonPositiveValues))]
        public void Create_WithNonPositiveValue_ShouldReturnExpectedError(int value)
        {
            // Act
            DomainResult<ChunkNumber> result = ChunkNumber.Create(value);

            // Assert
            result.ShouldFailWith(ChunkErrors.InvalidNumber);
        }

        [Fact]
        public void Next_ShouldReturnIncrementedNumber()
        {
            // Arrange
            ChunkNumber current = ChunkNumber.Create(5).ShouldSucceed();

            // Act
            ChunkNumber next = current.Next();

            // Assert
            current.Value.Should().Be(5);
            next.Value.Should().Be(6);
        }

        [Fact]
        public void Next_ShouldNotMutateOriginalNumber()
        {
            // Arrange
            ChunkNumber current = ChunkNumber.Create(5).ShouldSucceed();

            // Act
            _ = current.Next();

            // Assert
            current.Value.Should().Be(5);
        }

        [Fact]
        public void NumbersWithSameValue_ShouldBeEqual()
        {
            // Arrange
            ChunkNumber first = ChunkNumber.Create(3).ShouldSucceed();

            ChunkNumber second = ChunkNumber.Create(3).ShouldSucceed();

            // Assert
            first.Should().Be(second);

            first.GetHashCode().Should().Be(second.GetHashCode());
        }

        [Fact]
        public void NumbersWithDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            ChunkNumber first = ChunkNumber.Create(1).ShouldSucceed();

            ChunkNumber second = ChunkNumber.Create(2).ShouldSucceed();

            // Assert
            first.Should().NotBe(second);
        }
    }
}