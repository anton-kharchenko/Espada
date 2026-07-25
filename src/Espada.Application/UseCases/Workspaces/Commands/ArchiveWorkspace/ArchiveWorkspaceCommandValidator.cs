using FluentValidation;

namespace Espada.Application.UseCases.Workspaces.Commands.ArchiveWorkspace
{
    internal sealed class ArchiveWorkspaceCommandValidator : AbstractValidator<ArchiveWorkspaceCommand>
    {
        public ArchiveWorkspaceCommandValidator()
        {
            RuleFor(command => command.WorkspaceId).NotEmpty();
        }
    }
}