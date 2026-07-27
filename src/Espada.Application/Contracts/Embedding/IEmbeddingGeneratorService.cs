using Espada.Application.Models;

namespace Espada.Application.Contracts.Embedding;

public interface IEmbeddingGeneratorService
{
    Task<GeneratedEmbedding> GenerateAsync(
        string modelIdentifier,
        string modelVersion,
        string input,
        CancellationToken cancellationToken = default);
}