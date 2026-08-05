using Espada.Domain.ValueObjects;
using FluentValidation;

namespace Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace
{
    internal sealed class CreateWorkspaceCommandValidator : AbstractValidator<CreateWorkspaceCommand>
    {
        public CreateWorkspaceCommandValidator()
        {
            RuleFor(command => command.Name)
                .NotEmpty()
                .MaximumLength(WorkspaceName.MaxLength);

            RuleFor(command => command.Type)
                .NotNull();

            RuleFor(command => command)
                .Must(command => string.IsNullOrWhiteSpace(command.IdentityIssuer) ==
                                 string.IsNullOrWhiteSpace(command.IdentitySubject))
                .WithMessage("Identity issuer and subject must be provided together.");
        }
    }
}