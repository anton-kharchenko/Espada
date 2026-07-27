using Espada.Domain.ValueObjects;
using FluentValidation;

namespace Espada.Application.UseCases.Sources.Commands.SetSourcePriority;

internal sealed class SetSourcePriorityCommandValidator : AbstractValidator<SetSourcePriorityCommand>
{
    public SetSourcePriorityCommandValidator()
    {
        RuleFor(command => command.WorkspaceId).NotEmpty();
        RuleFor(command => command.SourceId).NotEmpty();
        RuleFor(command => command.Priority).InclusiveBetween(ContextPriority.Minimum, ContextPriority.Maximum);
    }
}