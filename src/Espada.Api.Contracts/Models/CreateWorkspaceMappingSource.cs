using Espada.Api.Contracts.Requests.Workspaces;

namespace Espada.Api.Contracts.Models
{
    public sealed record CreateWorkspaceMappingSource(
        CreateWorkspaceRequest Request,
        string? IdentityIssuer,
        string? IdentitySubject);
}