using Espada.Domain.ValueObjects;
using FluentValidation;

namespace Espada.Application.UseCases.Artifacts.Commands.SetArtifactPriority
{
    internal sealed class SetArtifactPriorityCommandValidator : AbstractValidator<SetArtifactPriorityCommand>
    {
        public SetArtifactPriorityCommandValidator()
        {
            RuleFor(command => command.WorkspaceId).NotEmpty();
            RuleFor(command => command.ArtifactId).NotEmpty();
            RuleFor(command => command.Priority).InclusiveBetween(ContextPriority.Minimum, ContextPriority.Maximum);
        }
    }
}