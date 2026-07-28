using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Memories.Queries.SearchMemory
{
    public sealed record SearchMemoryQuery(
        Guid WorkspaceId,
        string QueryText,
        IReadOnlyCollection<int>? CategoryTypeIds = null,
        int TopK = 10) : IQuery<SearchMemoryResponse>;
}