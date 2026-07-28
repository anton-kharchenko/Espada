using Espada.Application.Contracts.Embedding;
using Espada.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Espada.Infrastructure.Services
{
    internal sealed class EmbeddingModelDefaults(IOptions<EmbeddingGenerationOptions> options) : IEmbeddingModelDefaults
    {
        public string? DefaultModel => options.Value.DefaultModel;
    }
}