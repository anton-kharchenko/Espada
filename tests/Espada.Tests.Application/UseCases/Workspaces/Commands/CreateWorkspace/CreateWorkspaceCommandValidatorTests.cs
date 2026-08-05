using Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Workspaces.Commands.CreateWorkspace
{
    public sealed class CreateWorkspaceCommandValidatorTests
    {
        private readonly CreateWorkspaceCommandValidator _validator = new();

        [Fact]
        public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
        {
            // Arrange
            CreateWorkspaceCommand command = new CreateWorkspaceCommandBuilder().Build();

            // Act
            TestValidationResult<CreateWorkspaceCommand> result =
                await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
        public async Task Validate_WithEmptyName_ShouldHaveNameError(string? name)
        {
            // Arrange
            CreateWorkspaceCommand command = new CreateWorkspaceCommandBuilder().WithName(name).Build();

            // Act
            TestValidationResult<CreateWorkspaceCommand> result =
                await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(value => value.Name);
        }

        [Fact]
        public async Task Validate_WithNameAtMaximumLength_ShouldNotHaveNameError()
        {
            // Arrange
            string name = new('a', WorkspaceName.MaxLength);

            CreateWorkspaceCommand command = new CreateWorkspaceCommandBuilder().WithName(name).Build();

            // Act
            TestValidationResult<CreateWorkspaceCommand> result =
                await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveValidationErrorFor(value => value.Name);
        }

        [Fact]
        public async Task Validate_WithNameAboveMaximumLength_ShouldHaveNameError()
        {
            // Arrange
            string name = new('a', WorkspaceName.MaxLength + 1);

            CreateWorkspaceCommand command = new CreateWorkspaceCommandBuilder().WithName(name).Build();

            // Act
            TestValidationResult<CreateWorkspaceCommand> result =
                await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(value => value.Name);
        }

        [Fact]
        public async Task Validate_WithoutWorkspaceType_ShouldHaveTypeError()
        {
            // Arrange
            CreateWorkspaceCommand command = new CreateWorkspaceCommandBuilder().WithoutType().Build();

            // Act
            TestValidationResult<CreateWorkspaceCommand> result =
                await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(value => value.Type);
        }
    }
}