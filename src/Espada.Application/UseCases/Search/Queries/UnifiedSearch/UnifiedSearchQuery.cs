using Espada.Application.Contracts.Messaging;

namespace Espada.Application.UseCases.Search.Queries.UnifiedSearch
{
    public sealed record UnifiedSearchQuery(Guid WorkspaceId, string Query, int Limit = 20)
        : IQuery<UnifiedSearchResponse>;
}
