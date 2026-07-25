using FluentValidation;

namespace Espada.Application.UseCases.Imports.Commands.RequestImport
{
    internal sealed class RequestImportCommandValidator : AbstractValidator<RequestImportCommand>
    {
        public RequestImportCommandValidator()
        {
            RuleFor(command => command.WorkspaceId)
                .NotEmpty();

            RuleFor(command => command.SourceId)
                .NotEmpty();
        }
    }
}