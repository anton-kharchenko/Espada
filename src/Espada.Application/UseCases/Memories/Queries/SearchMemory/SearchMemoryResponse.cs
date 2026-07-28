namespace Espada.Application.UseCases.Memories.Queries.SearchMemory
{
    public sealed record SearchMemoryResponse(
        IReadOnlyList<MemorySearchItemResponse> Items);
}