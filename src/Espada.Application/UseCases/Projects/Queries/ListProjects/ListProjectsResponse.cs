using Espada.Application.UseCases.Projects.Common;

namespace Espada.Application.UseCases.Projects.Queries.ListProjects
{
    public sealed record ListProjectsResponse(
        IReadOnlyList<ProjectResponse> Items);
}