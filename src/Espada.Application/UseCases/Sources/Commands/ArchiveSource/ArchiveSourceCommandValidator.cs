using FluentValidation;

namespace Espada.Application.UseCases.Sources.Commands.ArchiveSource
{
    internal sealed class ArchiveSourceCommandValidator : AbstractValidator<ArchiveSourceCommand>
    {
        public ArchiveSourceCommandValidator()
        {
            RuleFor(command => command.WorkspaceId)
                .NotEmpty();

            RuleFor(command => command.SourceId)
                .NotEmpty();
        }
    }
}