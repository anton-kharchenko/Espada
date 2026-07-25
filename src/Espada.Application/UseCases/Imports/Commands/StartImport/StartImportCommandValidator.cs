using FluentValidation;

namespace Espada.Application.UseCases.Imports.Commands.StartImport;

internal sealed class StartImportCommandValidator
    : AbstractValidator<StartImportCommand>
{
    public StartImportCommandValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.ImportJobId)
            .NotEmpty();
    }
}