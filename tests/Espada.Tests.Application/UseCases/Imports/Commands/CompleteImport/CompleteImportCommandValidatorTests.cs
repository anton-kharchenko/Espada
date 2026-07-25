using Espada.Application.UseCases.Imports.Commands.CompleteImport;
using Espada.Tests.Application.TestData.Builder;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Imports.Commands.CompleteImport;

public sealed class CompleteImportCommandValidatorTests
{
    private readonly CompleteImportCommandValidator _validator =
        new();

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        CompleteImportCommand command = new CompleteImportCommandBuilder().Build();

        // Act
        TestValidationResult<CompleteImportCommand> result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
    {
        // Arrange
        CompleteImportCommand command = new CompleteImportCommandBuilder().InWorkspace(Guid.Empty).Build();

        // Act
        TestValidationResult<CompleteImportCommand> result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(value => value.WorkspaceId);

        result.ShouldNotHaveValidationErrorFor(value => value.ImportJobId);

        result.ShouldNotHaveValidationErrorFor(value => value.ArtifactId);

        result.ShouldNotHaveValidationErrorFor(value => value.ArtifactRevisionId);
    }

    [Fact]
    public async Task Validate_WithEmptyImportJobId_ShouldHaveError()
    {
        // Arrange
        CompleteImportCommand command = new CompleteImportCommandBuilder()
                .ForImportJob(Guid.Empty)
                .Build();

        // Act
        TestValidationResult<CompleteImportCommand> result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(value => value.ImportJobId);

        result.ShouldNotHaveValidationErrorFor(value => value.WorkspaceId);

        result.ShouldNotHaveValidationErrorFor(value => value.ArtifactId);

        result.ShouldNotHaveValidationErrorFor(value => value.ArtifactRevisionId);
    }

    [Fact]
    public async Task Validate_WithEmptyArtifactId_ShouldHaveError()
    {
        // Arrange
        CompleteImportCommand command = new CompleteImportCommandBuilder()
                .WithArtifact(Guid.Empty)
                .Build();

        // Act
        TestValidationResult<CompleteImportCommand> result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(value => value.ArtifactId);

        result.ShouldNotHaveValidationErrorFor(value => value.WorkspaceId);

        result.ShouldNotHaveValidationErrorFor(value => value.ImportJobId);

        result.ShouldNotHaveValidationErrorFor(value => value.ArtifactRevisionId);
    }

    [Fact]
    public async Task Validate_WithEmptyArtifactRevisionId_ShouldHaveError()
    {
        // Arrange
        CompleteImportCommand command = new CompleteImportCommandBuilder()
                .WithArtifactRevision(Guid.Empty)
                .Build();

        // Act
        TestValidationResult<CompleteImportCommand> result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(value => value.ArtifactRevisionId);

        result.ShouldNotHaveValidationErrorFor(value => value.WorkspaceId);

        result.ShouldNotHaveValidationErrorFor(value => value.ImportJobId);

        result.ShouldNotHaveValidationErrorFor(value => value.ArtifactId);
    }
}