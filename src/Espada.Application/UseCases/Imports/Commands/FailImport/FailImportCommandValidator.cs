using FluentValidation;

namespace Espada.Application.UseCases.Imports.Commands.FailImport
{
    internal sealed class FailImportCommandValidator : AbstractValidator<FailImportCommand>
    {
        public FailImportCommandValidator()
        {
            RuleFor(command => command.WorkspaceId)
                .NotEmpty();

            RuleFor(command => command.ImportJobId)
                .NotEmpty();

            RuleFor(command => command.FailureCode)
                .NotEmpty();

            RuleFor(command => command.FailureReason)
                .NotEmpty();
        }
    }
}