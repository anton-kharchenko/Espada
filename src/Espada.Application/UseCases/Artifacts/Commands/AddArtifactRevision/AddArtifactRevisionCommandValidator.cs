using FluentValidation;

namespace Espada.Application.UseCases.Artifacts.Commands.AddArtifactRevision
{
    internal sealed class AddArtifactRevisionCommandValidator
        : AbstractValidator<AddArtifactRevisionCommand>
    {
        public AddArtifactRevisionCommandValidator()
        {
            RuleFor(command => command.WorkspaceId)
                .NotEmpty();

            RuleFor(command => command.ArtifactId)
                .NotEmpty();

            RuleFor(command => command.Content)
                .Must(content => !string.IsNullOrWhiteSpace(content));
        }
    }
}