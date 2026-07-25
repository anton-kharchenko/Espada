using FluentValidation;

namespace Espada.Application.UseCases.Imports.Commands.CompleteImport
{
    internal sealed class CompleteImportCommandValidator
        : AbstractValidator<CompleteImportCommand>
    {
        public CompleteImportCommandValidator()
        {
            RuleFor(command => command.WorkspaceId)
                .NotEmpty();

            RuleFor(command => command.ImportJobId)
                .NotEmpty();

            RuleFor(command => command.ArtifactId)
                .NotEmpty();

            RuleFor(command => command.ArtifactRevisionId)
                .NotEmpty();
        }
    }
}