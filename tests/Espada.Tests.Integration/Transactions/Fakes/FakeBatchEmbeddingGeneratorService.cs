using Espada.Application.Contracts.Embedding;
using Espada.Application.Models;

namespace Espada.Tests.Integration.Transactions.Fakes;

internal sealed class FakeBatchEmbeddingGeneratorService : IBatchEmbeddingGeneratorService
{
    public Task<IReadOnlyList<GeneratedEmbedding>> GenerateBatchAsync(string modelIdentifier, string modelVersion, IReadOnlyList<string> inputs, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<GeneratedEmbedding> result =
        [
            .. inputs.Select(_ => new GeneratedEmbedding([1f, 0f, 0f]))
        ];

        return Task.FromResult(result);
    }
}