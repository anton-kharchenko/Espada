using Espada.Application.Contracts.Embedding;
using Espada.Application.Models;

namespace Espada.Tests.Infrastructure.Ingestion.Fakes
{
    internal sealed class TestBatchEmbeddingGeneratorService : IBatchEmbeddingGeneratorService
    {
        public Task<IReadOnlyList<GeneratedEmbedding>> GenerateBatchAsync(string modelIdentifier, string modelVersion,
            IReadOnlyList<string> inputs, CancellationToken cancellationToken = default)
        {
            GeneratedEmbedding[] vectors =
            [
                new([1f, 0f]),
                new([0.99f, 0.01f]),
                new([0f, 1f])
            ];

            return Task.FromResult<IReadOnlyList<GeneratedEmbedding>>(vectors);
        }
    }
}