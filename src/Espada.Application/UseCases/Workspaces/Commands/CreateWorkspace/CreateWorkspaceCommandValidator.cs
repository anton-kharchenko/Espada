using Espada.Domain.ValueObjects;
using FluentValidation;

namespace Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;

internal sealed class CreateWorkspaceCommandValidator : AbstractValidator<CreateWorkspaceCommand>
{
    public CreateWorkspaceCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(WorkspaceName.MaxLength);

        RuleFor(command => command.Type)
            .NotNull();
    }
}