using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors
{
    public static class WorkspaceApplicationErrors
    {
        public static readonly DomainError InvalidId = new("Workspace.Id.Invalid", "Workspace ID cannot be empty.");

        public static DomainError NotFound(Guid workspaceId) => 
            new("Workspace.NotFound", $"Workspace with ID '{workspaceId:D}' was not found.");
    }
}