using Espada.Domain.Errors;

namespace Espada.Tests.Domain.ValueObjects
{
    public sealed class ImportFailureTests
    {
        public static TheoryData<string?> EmptyValues =>
            new() { null!, string.Empty, " " };

        [Fact]
        public void Create_WithValidValues_ShouldCreateFailure()
        {
            // Act
            ImportFailure failure = ImportFailure.Create("source.read_failed", "The source could not be read.")
                .ShouldSucceed();

            // Assert
            failure.Code.Should().Be("source.read_failed");

            failure.Reason.Should().Be("The source could not be read.");
        }

        [Fact]
        public void Create_ShouldTrimCodeAndReason()
        {
            // Act
            ImportFailure failure = ImportFailure.Create("  source.read_failed  ", "  The source could not be read.  ")
                .ShouldSucceed();

            // Assert
            failure.Code.Should().Be("source.read_failed");
            failure.Reason.Should().Be("The source could not be read.");
        }

        [Theory]
        [MemberData(nameof(EmptyValues))]
        public void Create_WithEmptyCode_ShouldReturnExpectedError(string? code)
        {
            // Act
            DomainResult<ImportFailure> result = ImportFailure.Create(code, "Failure reason.");

            // Assert
            result.ShouldFailWith(ImportJobErrors.FailureCodeEmpty);
        }

        [Theory]
        [MemberData(nameof(EmptyValues))]
        public void Create_WithEmptyReason_ShouldReturnExpectedError(string? reason)
        {
            // Act
            DomainResult<ImportFailure> result = ImportFailure.Create("source.read_failed", reason);

            // Assert
            result.ShouldFailWith(ImportJobErrors.FailureReasonEmpty);
        }

        [Fact]
        public void Create_WithCodeAboveMaximumLength_ShouldReturnFailure()
        {
            // Arrange
            string code = new('a', ImportFailure.CodeMaxLength + 1);

            // Act
            DomainResult<ImportFailure> result = ImportFailure.Create(code, "Failure reason.");

            // Assert
            result.ShouldFailWith(ImportJobErrors.FailureCodeTooLong);
        }

        [Fact]
        public void Create_WithReasonAboveMaximumLength_ShouldReturnFailure()
        {
            // Arrange
            string reason = new('a', ImportFailure.ReasonMaxLength + 1);

            // Act
            DomainResult<ImportFailure> result = ImportFailure.Create("source.read_failed", reason);

            // Assert
            result.ShouldFailWith(ImportJobErrors.FailureReasonTooLong);
        }

        [Fact]
        public void FailuresWithSameValues_ShouldBeEqual()
        {
            // Arrange
            ImportFailure first = ImportFailure.Create("source.read_failed", "Failure reason.").ShouldSucceed();

            ImportFailure second = ImportFailure.Create("source.read_failed", "Failure reason.").ShouldSucceed();

            // Assert
            first.Should().Be(second);
            first.GetHashCode().Should().Be(second.GetHashCode());
        }
    }
}