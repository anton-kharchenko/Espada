using FluentValidation;

namespace Espada.Application.UseCases.Artifacts.Commands.ArchiveArtifact
{
    internal sealed class ArchiveArtifactCommandValidator
        : AbstractValidator<ArchiveArtifactCommand>
    {
        public ArchiveArtifactCommandValidator()
        {
            RuleFor(command => command.WorkspaceId)
                .NotEmpty();

            RuleFor(command => command.ArtifactId)
                .NotEmpty();
        }
    }
}