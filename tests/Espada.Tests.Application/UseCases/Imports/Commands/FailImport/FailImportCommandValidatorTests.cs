using Espada.Application.UseCases.Imports.Commands.FailImport;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Imports.Commands.FailImport
{
    public sealed class FailImportCommandValidatorTests
    {
        private readonly FailImportCommandValidator _validator = new();

        [Fact]
        public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
        {
            // Arrange
            FailImportCommand command = new FailImportCommandBuilder().Build();

            // Act
            TestValidationResult<FailImportCommand> result = await _validator.TestValidateAsync(
                command,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
        {
            // Arrange
            FailImportCommand command = new FailImportCommandBuilder()
                .InWorkspace(Guid.Empty)
                .Build();

            // Act
            TestValidationResult<FailImportCommand> result = await _validator.TestValidateAsync(
                command,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(failImportCommand => failImportCommand.WorkspaceId);
        }

        [Fact]
        public async Task Validate_WithEmptyImportJobId_ShouldHaveError()
        {
            // Arrange
            FailImportCommand command = new FailImportCommandBuilder()
                .ForImportJob(Guid.Empty)
                .Build();

            // Act
            TestValidationResult<FailImportCommand> result = await _validator.TestValidateAsync(
                command,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(failImportCommand => failImportCommand.ImportJobId);
        }

        [Theory]
        [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
        public async Task Validate_WithEmptyFailureCode_ShouldHaveError(string? failureCode)
        {
            // Arrange
            FailImportCommand command = new FailImportCommandBuilder()
                .WithFailureCode(failureCode)
                .Build();

            // Act
            TestValidationResult<FailImportCommand> result = await _validator.TestValidateAsync(
                command,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(failImportCommand => failImportCommand.FailureCode);
        }

        [Theory]
        [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
        public async Task Validate_WithEmptyFailureReason_ShouldHaveError(string? failureReason)
        {
            // Arrange
            FailImportCommand command = new FailImportCommandBuilder()
                .WithFailureReason(failureReason)
                .Build();

            // Act
            TestValidationResult<FailImportCommand> result = await _validator.TestValidateAsync(
                command,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(failImportCommand => failImportCommand.FailureReason);
        }
    }
}