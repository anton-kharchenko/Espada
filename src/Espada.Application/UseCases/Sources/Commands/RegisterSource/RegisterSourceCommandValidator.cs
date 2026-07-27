using Espada.Domain.ValueObjects.SourceDefinitions;
using FluentValidation;

namespace Espada.Application.UseCases.Sources.Commands.RegisterSource;

internal sealed class RegisterSourceCommandValidator : AbstractValidator<RegisterSourceCommand>
{
    public RegisterSourceCommandValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty();

        RuleFor(command => command.Definition)
            .NotNull()
            .Must(definition => definition is not LegacySourceDefinition);
    }
}