using Espada.Application.Contracts.Embedding;

namespace Espada.Tests.Application.Fakes
{
    internal sealed class TestEmbeddingModelDefaults : IEmbeddingModelDefaults
    {
        public string? DefaultModel { get; set; }
    }
}