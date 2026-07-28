using Espada.Domain.Errors;

namespace Espada.Tests.Domain.ValueObjects
{
    public sealed class WorkspaceNameTests
    {
        [Fact]
        public void Create_WithValidValue_ShouldCreateName()
        {
            // Act
            DomainResult<WorkspaceName> result = WorkspaceName.Create("Espada Workspace");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value?.Value.Should().Be("Espada Workspace");
        }

        [Fact]
        public void Create_WithSurroundingWhitespace_ShouldNormalizeName()
        {
            // Act
            DomainResult<WorkspaceName> result = WorkspaceName.Create("  Espada Workspace  ");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value?.Value.Should().Be("Espada Workspace");
        }

        [Theory]
        [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
        public void Create_WithEmptyValue_ShouldReturnExpectedError(string? value)
        {
            // Act
            DomainResult<WorkspaceName> result = WorkspaceName.Create(value);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(WorkspaceErrors.NameEmpty);
        }

        [Fact]
        public void Create_WithMaximumLength_ShouldSucceed()
        {
            // Arrange
            string value = new('a', WorkspaceName.MaxLength);

            // Act
            DomainResult<WorkspaceName> result = WorkspaceName.Create(value);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value?.Value.Should().HaveLength(WorkspaceName.MaxLength);
        }

        [Fact]
        public void Create_AboveMaximumLength_ShouldReturnExpectedError()
        {
            // Arrange
            string value = new('a', WorkspaceName.MaxLength + 1);

            // Act
            DomainResult<WorkspaceName> result = WorkspaceName.Create(value);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(WorkspaceErrors.NameTooLong);
        }

        [Fact]
        public void Equality_WithSameValue_ShouldBeEqual()
        {
            // Arrange
            WorkspaceName? first = WorkspaceName.Create("Espada").Value;
            WorkspaceName? second = WorkspaceName.Create("Espada").Value;

            // Assert
            first.Should().Be(second);
            first.GetHashCode().Should().Be(second.GetHashCode());
        }
    }
}