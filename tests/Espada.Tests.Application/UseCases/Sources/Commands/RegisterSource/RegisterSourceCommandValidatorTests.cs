using Espada.Application.UseCases.Sources.Commands.RegisterSource;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Sources.Commands.RegisterSource;

public sealed class RegisterSourceCommandValidatorTests
{
    private readonly RegisterSourceCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        RegisterSourceCommand command = new RegisterSourceCommandBuilder().Build();

        // Act
        TestValidationResult<RegisterSourceCommand> result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
    {
        // Arrange
        RegisterSourceCommand command = new RegisterSourceCommandBuilder().InWorkspace(Guid.Empty).Build();

        // Act
        TestValidationResult<RegisterSourceCommand> result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(value => value.WorkspaceId);
    }

    [Theory]
    [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
    public async Task Validate_WithEmptyName_ShouldHaveError(string name)
    {
        // Arrange
        RegisterSourceCommand command = new RegisterSourceCommandBuilder().WithName(name).Build();

        // Act
        TestValidationResult<RegisterSourceCommand> result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(value => value.Name);
    }

    [Theory]
    [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
    public async Task Validate_WithEmptyLocator_ShouldHaveError(string locator)
    {
        // Arrange
        RegisterSourceCommand command = new RegisterSourceCommandBuilder().WithLocator(locator).Build();

        // Act
        TestValidationResult<RegisterSourceCommand> result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(value => value.Locator);
    }

    [Fact]
    public async Task Validate_WithoutType_ShouldHaveError()
    {
        // Arrange
        RegisterSourceCommand command = new RegisterSourceCommandBuilder().WithoutType().Build();

        // Act
        TestValidationResult<RegisterSourceCommand> result = await _validator.TestValidateAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(value => value.Type);
    }
}