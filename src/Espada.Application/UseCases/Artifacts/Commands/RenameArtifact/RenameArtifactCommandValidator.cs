using Espada.Domain.ValueObjects;
using FluentValidation;

namespace Espada.Application.UseCases.Artifacts.Commands.RenameArtifact
{
    internal sealed class RenameArtifactCommandValidator
        : AbstractValidator<RenameArtifactCommand>
    {
        public RenameArtifactCommandValidator()
        {
            RuleFor(command => command.WorkspaceId)
                .NotEmpty();

            RuleFor(command => command.ArtifactId)
                .NotEmpty();

            RuleFor(command => command.Title)
                .Must(title => !string.IsNullOrWhiteSpace(title))
                .MaximumLength(ArtifactTitle.MaxLength);
        }
    }
}