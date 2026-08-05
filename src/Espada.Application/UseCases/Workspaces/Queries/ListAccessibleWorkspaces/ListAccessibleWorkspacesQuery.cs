using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Workspaces.Queries.ListAccessibleWorkspaces
{
    public sealed record ListAccessibleWorkspacesQuery(
        string IdentityIssuer,
        string IdentitySubject) : IQuery<ListAccessibleWorkspacesResponse>;
}