using Espada.Application.Models;

namespace Espada.Application.Contracts.Embedding
{
    public interface IBatchEmbeddingGeneratorService
    {
        Task<IReadOnlyList<GeneratedEmbedding>> GenerateBatchAsync(
            string modelIdentifier,
            string modelVersion,
            IReadOnlyList<string> inputs,
            CancellationToken cancellationToken = default);
    }
}