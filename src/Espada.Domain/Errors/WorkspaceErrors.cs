using Espada.Domain.Rules;
using Espada.Domain.ValueObjects;

namespace Espada.Domain.Errors;

public static class WorkspaceErrors
{
    public static readonly DomainError NameEmpty =
        new("Workspace.Name.Empty", "Workspace name is required.");

    public static readonly DomainError NameTooLong =
        new("Workspace.Name.TooLong", $"Workspace name cannot exceed {WorkspaceName.MaxLength} characters.");
}