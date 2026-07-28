using Espada.Application.Contracts.Embedding;
using Espada.Application.Models;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class EmbeddingGeneratorServiceSpy : IEmbeddingGeneratorService
    {
        public int GenerateCallCount { get; private set; }
        public string? ReceivedModelIdentifier { get; private set; }
        public string? ReceivedModelVersion { get; private set; }
        public GeneratedEmbedding EmbeddingToReturn { get; set; } = new([1f, 0f, 0f]);

        public Task<GeneratedEmbedding> GenerateAsync(
            string modelIdentifier,
            string modelVersion,
            string input,
            CancellationToken cancellationToken = default)
        {
            GenerateCallCount++;
            ReceivedModelIdentifier = modelIdentifier;
            ReceivedModelVersion = modelVersion;
            return Task.FromResult(EmbeddingToReturn);
        }
    }
}