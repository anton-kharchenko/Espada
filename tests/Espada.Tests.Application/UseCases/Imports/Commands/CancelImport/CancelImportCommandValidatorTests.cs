using Espada.Application.UseCases.Imports.Commands.CancelImport;
using Espada.Tests.Application.TestData.Builder;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Imports.Commands.CancelImport
{
    public sealed class CancelImportCommandValidatorTests
    {
        private readonly CancelImportCommandValidator _validator = new();

        [Fact]
        public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
        {
            // Arrange
            CancelImportCommand command = new CancelImportCommandBuilder().Build();

            // Act
            TestValidationResult<CancelImportCommand> result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
        {
            // Arrange
            CancelImportCommand command = new CancelImportCommandBuilder()
                .InWorkspace(Guid.Empty)
                .Build();

            // Act
            TestValidationResult<CancelImportCommand> result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(cancelImportCommand => cancelImportCommand.WorkspaceId);
        }

        [Fact]
        public async Task Validate_WithEmptyImportJobId_ShouldHaveError()
        {
            // Arrange
            CancelImportCommand command = new CancelImportCommandBuilder()
                .ForImportJob(Guid.Empty)
                .Build();

            // Act
            TestValidationResult<CancelImportCommand> result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(cancelImportCommand => cancelImportCommand.ImportJobId);
        }
    }
}