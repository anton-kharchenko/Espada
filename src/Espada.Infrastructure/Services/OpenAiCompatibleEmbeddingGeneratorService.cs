using Espada.Application.Contracts.Embedding;
using Espada.Application.Models;
using Espada.Infrastructure.Models;
using Espada.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Espada.Infrastructure.Services;

internal sealed class OpenAiCompatibleEmbeddingGeneratorService(HttpClient httpClient, IOptions<EmbeddingGenerationOptions> options) : IEmbeddingGeneratorService
{
    private readonly EmbeddingGenerationOptions _options = options.Value;

    public async Task<GeneratedEmbedding> GenerateAsync(string modelIdentifier, string modelVersion, string input, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelIdentifier);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        EmbeddingModelOptions model = _options.Models.SingleOrDefault(candidate =>
            candidate.Identifier.Equals(modelIdentifier, StringComparison.Ordinal) &&
            candidate.Version.Equals(modelVersion, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Embedding model '{modelIdentifier}@{modelVersion}' is not allowlisted.");

        if (!Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out Uri? baseUri))
        {
            throw new InvalidOperationException("EmbeddingGeneration:BaseUrl must be an absolute URI.");
        }

        Uri endpoint = new($"{baseUri.ToString().TrimEnd('/')}/v1/embeddings");
        using HttpRequestMessage request = new(HttpMethod.Post, endpoint);
        request.Content = JsonContent.Create(new EmbeddingRequest(model.ProviderModel, input));

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }

        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        EmbeddingResponse? payload = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(cancellationToken);
        float[] vector = payload?.Data?.FirstOrDefault()?.Embedding ?? throw new InvalidOperationException("Embedding provider returned no vector.");

        if (vector.Length != model.Dimensions)
        {
            throw new InvalidOperationException($"Embedding provider returned {vector.Length} dimensions for '{modelIdentifier}@{modelVersion}', expected {model.Dimensions}.");
        }

        return vector.Any(value => !float.IsFinite(value)) ? throw new InvalidOperationException("Embedding provider returned a non-finite vector value.") : new GeneratedEmbedding(vector);
    }
}