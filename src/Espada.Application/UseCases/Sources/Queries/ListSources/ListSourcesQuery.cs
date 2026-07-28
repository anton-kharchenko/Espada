using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Sources.Queries.ListSources
{
    public sealed record ListSourcesQuery(
        Guid WorkspaceId) : IQuery<ListSourcesResponse>;
}
