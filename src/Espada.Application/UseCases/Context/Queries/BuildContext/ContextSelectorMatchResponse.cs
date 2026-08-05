namespace Espada.Application.UseCases.Context.Queries.BuildContext
{
    public sealed record ContextSelectorMatchResponse(
        string Selector,
        string? Expected,
        string? Actual,
        bool Matched);
}