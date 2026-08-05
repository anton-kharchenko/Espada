using Espada.Domain.ValueObjects;

namespace Espada.Application.Models
{
    public sealed record EmbeddingVectorSearch(
        WorkspaceId WorkspaceId,
        EmbeddingModel Model,
        IReadOnlyList<float> QueryVector,
        int TopK,
        double? MinimumSimilarity = null);
}