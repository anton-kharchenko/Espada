using Espada.Application.UseCases.Artifacts.Commands.RenameArtifact;
using Espada.Domain.ValueObjects;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Artifacts.Commands.RenameArtifact
{
    public sealed class RenameArtifactCommandValidatorTests
    {
        private readonly RenameArtifactCommandValidator _validator = new();

        [Fact]
        public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
        {
            RenameArtifactCommand command =
                new RenameArtifactCommandBuilder().Build();

            TestValidationResult<RenameArtifactCommand> result =
                await _validator.TestValidateAsync(
                    command,
                    cancellationToken:
                    TestContext.Current.CancellationToken);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
        {
            RenameArtifactCommand command =
                new RenameArtifactCommandBuilder()
                    .InWorkspace(Guid.Empty)
                    .Build();

            TestValidationResult<RenameArtifactCommand> result =
                await _validator.TestValidateAsync(
                    command,
                    cancellationToken:
                    TestContext.Current.CancellationToken);

            result.ShouldHaveValidationErrorFor(command => command.WorkspaceId);
        }

        [Fact]
        public async Task Validate_WithEmptyArtifactId_ShouldHaveError()
        {
            RenameArtifactCommand command =
                new RenameArtifactCommandBuilder()
                    .ForArtifact(Guid.Empty)
                    .Build();

            TestValidationResult<RenameArtifactCommand> result =
                await _validator.TestValidateAsync(
                    command,
                    cancellationToken:
                    TestContext.Current.CancellationToken);

            result.ShouldHaveValidationErrorFor(command => command.ArtifactId);
        }

        [Theory]
        [MemberData(
            nameof(StringTheoryData.NullOrWhiteSpaceValues),
            MemberType = typeof(StringTheoryData))]
        public async Task Validate_WithEmptyTitle_ShouldHaveError(
            string? title)
        {
            RenameArtifactCommand command =
                new RenameArtifactCommandBuilder()
                    .WithTitle(title)
                    .Build();

            TestValidationResult<RenameArtifactCommand> result =
                await _validator.TestValidateAsync(
                    command,
                    cancellationToken:
                    TestContext.Current.CancellationToken);

            result.ShouldHaveValidationErrorFor(command => command.Title);
        }

        [Fact]
        public async Task Validate_WithTitleTooLong_ShouldHaveError()
        {
            RenameArtifactCommand command =
                new RenameArtifactCommandBuilder()
                    .WithTitle(
                        new string('a', ArtifactTitle.MaxLength + 1))
                    .Build();

            TestValidationResult<RenameArtifactCommand> result =
                await _validator.TestValidateAsync(
                    command,
                    cancellationToken:
                    TestContext.Current.CancellationToken);

            result.ShouldHaveValidationErrorFor(command => command.Title);
        }
    }
}