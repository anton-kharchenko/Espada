namespace Espada.Application.UseCases.Imports.Queries.ListImports
{
    public sealed record ListImportsResponse(
        IReadOnlyList<ImportListItemResponse> Items);
}