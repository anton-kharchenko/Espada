namespace Espada.Application.UseCases.Search.Queries.UnifiedSearch
{
    public sealed record UnifiedSearchResponse(IReadOnlyList<UnifiedSearchItemResponse> Items);
}