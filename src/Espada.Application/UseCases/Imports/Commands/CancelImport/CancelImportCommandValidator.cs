using FluentValidation;

namespace Espada.Application.UseCases.Imports.Commands.CancelImport
{
    internal sealed class CancelImportCommandValidator : AbstractValidator<CancelImportCommand>
    {
        public CancelImportCommandValidator()
        {
            RuleFor(command => command.WorkspaceId)
                .NotEmpty();

            RuleFor(command => command.ImportJobId)
                .NotEmpty();
        }
    }
}