using Espada.Application.UseCases.Artifacts.Commands.ArchiveArtifact;
using Espada.Tests.Application.TestData.Builder;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Artifacts.Commands.ArchiveArtifact
{
    public sealed class ArchiveArtifactCommandValidatorTests
    {
        private readonly ArchiveArtifactCommandValidator _validator = new();

        [Fact]
        public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
        {
            ArchiveArtifactCommand command =
                new ArchiveArtifactCommandBuilder().Build();

            TestValidationResult<ArchiveArtifactCommand> result =
                await _validator.TestValidateAsync(
                    command,
                    cancellationToken:
                        TestContext.Current.CancellationToken);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
        {
            ArchiveArtifactCommand command =
                new ArchiveArtifactCommandBuilder()
                    .InWorkspace(Guid.Empty)
                    .Build();

            TestValidationResult<ArchiveArtifactCommand> result =
                await _validator.TestValidateAsync(
                    command,
                    cancellationToken:
                        TestContext.Current.CancellationToken);

            result.ShouldHaveValidationErrorFor(
                command => command.WorkspaceId);
        }

        [Fact]
        public async Task Validate_WithEmptyArtifactId_ShouldHaveError()
        {
            ArchiveArtifactCommand command =
                new ArchiveArtifactCommandBuilder()
                    .ForArtifact(Guid.Empty)
                    .Build();

            TestValidationResult<ArchiveArtifactCommand> result =
                await _validator.TestValidateAsync(
                    command,
                    cancellationToken:
                        TestContext.Current.CancellationToken);

            result.ShouldHaveValidationErrorFor(
                command => command.ArtifactId);
        }
    }
}