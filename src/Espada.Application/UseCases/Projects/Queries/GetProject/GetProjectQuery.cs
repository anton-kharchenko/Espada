using Espada.Application.Contracts.Messaging;
using Espada.Application.UseCases.Projects.Common;

namespace Espada.Application.UseCases.Projects.Queries.GetProject
{
    public sealed record GetProjectQuery(
        Guid WorkspaceId,
        Guid ProjectId) : IQuery<ProjectResponse>;
}