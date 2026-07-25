using Espada.Application.UseCases.Artifacts.Commands.CreateArtifact;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Artifacts.Commands.CreateArtifact
{
    public sealed class CreateArtifactCommandValidatorTests
    {
        private readonly CreateArtifactCommandValidator _validator = new();

        [Fact]
        public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
        {
            // Arrange
            CreateArtifactCommand command = new CreateArtifactCommandBuilder().Build();

            // Act
            TestValidationResult<CreateArtifactCommand> result = await _validator.TestValidateAsync(
                command,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
        {
            // Arrange
            CreateArtifactCommand command = new CreateArtifactCommandBuilder()
                .InWorkspace(Guid.Empty)
                .Build();

            // Act
            TestValidationResult<CreateArtifactCommand> result = await _validator.TestValidateAsync(
                command,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(createArtifactCommand => createArtifactCommand.WorkspaceId);
        }

        [Theory]
        [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
        public async Task Validate_WithEmptyTitle_ShouldHaveError(string? title)
        {
            // Arrange
            CreateArtifactCommand command = new CreateArtifactCommandBuilder()
                .WithTitle(title)
                .Build();

            // Act
            TestValidationResult<CreateArtifactCommand> result = await _validator.TestValidateAsync(
                command,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(createArtifactCommand => createArtifactCommand.Title);
        }

        [Fact]
        public async Task Validate_WithTitleTooLong_ShouldHaveError()
        {
            // Arrange
            CreateArtifactCommand command = new CreateArtifactCommandBuilder()
                .WithTitle(new string('a', ArtifactTitle.MaxLength + 1))
                .Build();

            // Act
            TestValidationResult<CreateArtifactCommand> result = await _validator.TestValidateAsync(
                command,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(createArtifactCommand => createArtifactCommand.Title);
        }

        [Fact]
        public async Task Validate_WithUnsupportedType_ShouldHaveError()
        {
            // Arrange
            CreateArtifactCommand command = new CreateArtifactCommandBuilder()
                .WithType(999)
                .Build();

            // Act
            TestValidationResult<CreateArtifactCommand> result = await _validator.TestValidateAsync(
                command,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(createArtifactCommand => createArtifactCommand.TypeId);
        }

        [Theory]
        [MemberData(nameof(StringTheoryData.NullOrWhiteSpaceValues), MemberType = typeof(StringTheoryData))]
        public async Task Validate_WithEmptyContent_ShouldHaveError(string? content)
        {
            // Arrange
            CreateArtifactCommand command = new CreateArtifactCommandBuilder()
                .WithContent(content)
                .Build();

            // Act
            TestValidationResult<CreateArtifactCommand> result = await _validator.TestValidateAsync(
                command,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldHaveValidationErrorFor(createArtifactCommand => createArtifactCommand.Content);
        }
    }
}