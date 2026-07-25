using Espada.Application.Contracts.Messaging;
using Espada.Application.UseCases.Workspaces.Common;

namespace Espada.Application.UseCases.Workspaces.Queries.GetWorkspaceById
{
    public sealed record GetWorkspaceByIdQuery(Guid WorkspaceId) : IQuery<WorkspaceResponse>;
}