using Espada.Application.UseCases.Workspaces.Commands.ArchiveWorkspace;
using Espada.Tests.Application.TestData;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Workspaces.Commands.ArchiveWorkspace
{
    public sealed class ArchiveWorkspaceCommandValidatorTests
    {
        private readonly ArchiveWorkspaceCommandValidator _validator = new();

        [Fact]
        public async Task Validate_WithValidWorkspaceId_ShouldNotHaveErrors()
        {
            // Arrange
            ArchiveWorkspaceCommand command = new(TestIds.DefaultWorkspaceId.Value);

            // Act
            TestValidationResult<ArchiveWorkspaceCommand> result =
                await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
        {
            // Arrange
            ArchiveWorkspaceCommand command = new(Guid.Empty);

            // Act
            TestValidationResult<ArchiveWorkspaceCommand> result =
                await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(value => value.WorkspaceId);
        }
    }
}