using Espada.Application.UseCases.Sources.Commands.ArchiveSource;
using Espada.Tests.Application.TestData;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Sources.Commands.ArchiveSource
{
    public sealed class ArchiveSourceCommandValidatorTests
    {
        private readonly ArchiveSourceCommandValidator _validator = new();

        [Fact]
        public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
        {
            // Arrange
            ArchiveSourceCommand command = new(TestIds.DefaultWorkspaceId.Value, TestIds.SourceId.Value);

            // Act
            TestValidationResult<ArchiveSourceCommand> result =
                await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
        {
            // Arrange
            ArchiveSourceCommand command = new(Guid.Empty, TestIds.SourceId.Value);

            // Act
            TestValidationResult<ArchiveSourceCommand> result =
                await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(value => value.WorkspaceId);
        }

        [Fact]
        public async Task Validate_WithEmptySourceId_ShouldHaveError()
        {
            // Arrange
            ArchiveSourceCommand command = new(TestIds.DefaultWorkspaceId.Value, Guid.Empty);

            // Act
            TestValidationResult<ArchiveSourceCommand> result =
                await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(value => value.SourceId);
        }
    }
}