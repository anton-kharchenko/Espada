using Espada.Application.UseCases.Artifacts.Commands.AddArtifactRevision;
using Espada.Tests.Application.TestData;
using Espada.Tests.Application.TestData.Builder;
using FluentValidation.TestHelper;

namespace Espada.Tests.Application.UseCases.Artifacts.Commands.AddArtifactRevision
{
    public sealed class AddArtifactRevisionCommandValidatorTests
    {
        private readonly AddArtifactRevisionCommandValidator _validator =
            new();

        [Fact]
        public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
        {
            AddArtifactRevisionCommand command =
                new AddArtifactRevisionCommandBuilder().Build();

            TestValidationResult<AddArtifactRevisionCommand> result =
                await _validator.TestValidateAsync(
                    command,
                    cancellationToken:
                        TestContext.Current.CancellationToken);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyWorkspaceId_ShouldHaveError()
        {
            AddArtifactRevisionCommand command =
                new AddArtifactRevisionCommandBuilder()
                    .InWorkspace(Guid.Empty)
                    .Build();

            TestValidationResult<AddArtifactRevisionCommand> result =
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
            AddArtifactRevisionCommand command =
                new AddArtifactRevisionCommandBuilder()
                    .ForArtifact(Guid.Empty)
                    .Build();

            TestValidationResult<AddArtifactRevisionCommand> result =
                await _validator.TestValidateAsync(
                    command,
                    cancellationToken:
                        TestContext.Current.CancellationToken);

            result.ShouldHaveValidationErrorFor(
                command => command.ArtifactId);
        }

        [Theory]
        [MemberData(
            nameof(StringTheoryData.NullOrWhiteSpaceValues),
            MemberType = typeof(StringTheoryData))]
        public async Task Validate_WithEmptyContent_ShouldHaveError(
            string? content)
        {
            AddArtifactRevisionCommand command =
                new AddArtifactRevisionCommandBuilder()
                    .WithContent(content)
                    .Build();

            TestValidationResult<AddArtifactRevisionCommand> result =
                await _validator.TestValidateAsync(
                    command,
                    cancellationToken:
                        TestContext.Current.CancellationToken);

            result.ShouldHaveValidationErrorFor(
                command => command.Content);
        }
    }
}