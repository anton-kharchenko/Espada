using Espada.Application.UseCases.Imports.Commands.StartImport;
using Espada.Tests.Application.TestData;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Imports.Commands.StartImport;

public sealed class StartImportCommandValidatorTests
{
    private readonly StartImportCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        StartImportCommand command = new(TestIds.WorkspaceId.Value, TestIds.DefaultImportJobId.Value);

        // Act
        TestValidationResult<StartImportCommand> result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
    {
        // Arrange
        StartImportCommand command = new(Guid.Empty, TestIds.DefaultImportJobId.Value);
        
        // Act
        TestValidationResult<StartImportCommand> result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(startImportCommand => startImportCommand.WorkspaceId);
    }

    [Fact]
    public async Task Validate_WithEmptyImportJobId_ShouldHaveError()
    {
        // Arrange
        StartImportCommand command = new(TestIds.WorkspaceId.Value, Guid.Empty);

        // Act
        TestValidationResult<StartImportCommand> result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(startImportCommand => startImportCommand.ImportJobId);
    }
}