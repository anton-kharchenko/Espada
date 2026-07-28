using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Tests.Application.TestData.Builder;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Imports.Commands.RequestImport
{
    public sealed class RequestImportCommandValidatorTests
    {
        private readonly RequestImportCommandValidator _validator =
            new();

        [Fact]
        public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
        {
            // Arrange
            RequestImportCommand command = new RequestImportCommandBuilder().Build();

            // Act
            TestValidationResult<RequestImportCommand> result = await _validator.TestValidateAsync(
                command,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
        {
            // Arrange
            RequestImportCommand command = new RequestImportCommandBuilder().InWorkspace(Guid.Empty).Build();

            // Act
            TestValidationResult<RequestImportCommand> result = await _validator.TestValidateAsync(
                command,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(value => value.WorkspaceId);
        }

        [Fact]
        public async Task Validate_WithEmptySourceId_ShouldHaveError()
        {
            // Arrange
            RequestImportCommand command = new RequestImportCommandBuilder().ForSource(Guid.Empty).Build();

            // Act
            TestValidationResult<RequestImportCommand> result = await _validator.TestValidateAsync(
                command,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(value => value.SourceId);
        }

        [Fact]
        public async Task Validate_WithEmptyIdempotencyKey_ShouldHaveError()
        {
            RequestImportCommand command = new RequestImportCommandBuilder()
                .WithIdempotencyKey(string.Empty)
                .Build();

            TestValidationResult<RequestImportCommand> result = await _validator.TestValidateAsync(
                command,
                cancellationToken: TestContext.Current.CancellationToken);

            result.ShouldHaveValidationErrorFor(value => value.IdempotencyKey);
        }
    }
}