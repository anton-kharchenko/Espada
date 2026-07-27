using FluentValidation;

namespace Espada.Application.UseCases.Imports.Commands.RequestImport;

internal sealed class RequestImportCommandValidator : AbstractValidator<RequestImportCommand>
{
    public RequestImportCommandValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.SourceId)
            .NotEmpty();

        RuleFor(command => command.IdempotencyKey)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Options)
            .NotNull();

        RuleFor(command => command.Options.MaxCharacters)
            .InclusiveBetween(1, 100_000);

        RuleFor(command => command.Options.OverlapCharacters)
            .GreaterThanOrEqualTo(0)
            .LessThan(command => command.Options.MaxCharacters);

        RuleFor(command => command.Options.SemanticThreshold)
            .InclusiveBetween(0, 1);
    }
}