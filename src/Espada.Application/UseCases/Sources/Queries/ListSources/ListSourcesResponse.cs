using Espada.Application.UseCases.Sources.Common;

namespace Espada.Application.UseCases.Sources.Queries.ListSources
{
    public sealed record ListSourcesResponse(
        IReadOnlyList<SourceResponse> Items);
}