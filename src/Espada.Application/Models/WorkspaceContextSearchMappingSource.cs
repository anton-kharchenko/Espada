using Espada.Application.UseCases.Context.Queries.SearchWorkspaceContext;
using Espada.Domain.ValueObjects;

namespace Espada.Application.Models
{
    internal sealed record WorkspaceContextSearchMappingSource(SearchWorkspaceContextQuery Query, EmbeddingModel Model, DateTimeOffset NowUtc);
}