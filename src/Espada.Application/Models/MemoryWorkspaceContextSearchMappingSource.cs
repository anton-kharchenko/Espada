using Espada.Application.UseCases.Memories.Queries.SearchMemory;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Models
{
    internal sealed record MemoryWorkspaceContextSearchMappingSource(
        SearchMemoryQuery Query,
        EmbeddingModel? Model,
        IReadOnlyList<float> QueryVector,
        IReadOnlyCollection<string> MemoryCategories,
        DateTimeOffset NowUtc);
}