using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Projects.Queries.ListProjects
{
    public sealed record ListProjectsQuery(
        Guid WorkspaceId) : IQuery<ListProjectsResponse>;
}