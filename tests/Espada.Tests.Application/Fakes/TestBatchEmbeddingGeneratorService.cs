using Espada.Application.Contracts.Embedding;
using Espada.Application.Models;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class TestBatchEmbeddingGeneratorService : IBatchEmbeddingGeneratorService
    {
        public Task<IReadOnlyList<GeneratedEmbedding>> GenerateBatchAsync(
            string modelIdentifier,
            string modelVersion,
            IReadOnlyList<string> inputs,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<GeneratedEmbedding> embeddings = inputs
                .Select(_ => new GeneratedEmbedding([1f, 0f, 0f], 1))
                .ToArray();
            return Task.FromResult(embeddings);
        }
    }
}