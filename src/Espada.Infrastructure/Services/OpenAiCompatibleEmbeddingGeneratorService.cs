using Espada.Application.Contracts.Embedding;
using Espada.Application.Models;
using Espada.Infrastructure.Models;
using Espada.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Espada.Infrastructure.Requests;
using Espada.Infrastructure.Responses;

namespace Espada.Infrastructure.Services
{
    internal sealed class OpenAiCompatibleEmbeddingGeneratorService(
        HttpClient httpClient,
        IOptions<EmbeddingGenerationOptions> options) : IEmbeddingGeneratorService, IBatchEmbeddingGeneratorService
    {
        private readonly EmbeddingGenerationOptions _options = options.Value;

        public Task<IReadOnlyList<GeneratedEmbedding>> GenerateBatchAsync(string modelIdentifier, string modelVersion,
            IReadOnlyList<string> inputs, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(inputs);
            if (inputs.Count == 0 || inputs.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Embedding inputs must not be empty.", nameof(inputs));
            }

            return GenerateCoreAsync(modelIdentifier, modelVersion, inputs, cancellationToken);
        }

        public async Task<GeneratedEmbedding> GenerateAsync(string modelIdentifier, string modelVersion, string input,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(modelIdentifier);
            ArgumentException.ThrowIfNullOrWhiteSpace(modelVersion);
            ArgumentException.ThrowIfNullOrWhiteSpace(input);

            IReadOnlyList<GeneratedEmbedding> generated =
                await GenerateCoreAsync(modelIdentifier, modelVersion, [input], cancellationToken);
            return generated[0];
        }

        private async Task<IReadOnlyList<GeneratedEmbedding>> GenerateCoreAsync(string modelIdentifier,
            string modelVersion, IReadOnlyList<string> inputs, CancellationToken cancellationToken)
        {
            EmbeddingModelOptions model = _options.Models.SingleOrDefault(candidate =>
                                              candidate.Identifier.Equals(modelIdentifier, StringComparison.Ordinal) &&
                                              candidate.Version.Equals(modelVersion, StringComparison.Ordinal))
                                          ?? throw new InvalidOperationException(
                                              $"Embedding model '{modelIdentifier}@{modelVersion}' is not allowlisted.");

            if (!Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out Uri? baseUri))
            {
                throw new InvalidOperationException("EmbeddingGeneration:BaseUrl must be an absolute URI.");
            }

            Uri endpoint = new($"{baseUri.ToString().TrimEnd('/')}/v1/embeddings");
            using HttpRequestMessage request = new(HttpMethod.Post, endpoint);
            request.Content =
                JsonContent.Create(new EmbeddingRequest(model.ProviderModel, inputs.Count == 1 ? inputs[0] : inputs));

            if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            }

            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            EmbeddingResponse? payload = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(cancellationToken);
            IReadOnlyList<EmbeddingData> data = payload?.Data
                                                ?? throw new InvalidOperationException(inputs.Count == 1
                                                    ? "Embedding provider returned no vector."
                                                    : "Embedding provider returned no vectors.");
            if (data.Count != inputs.Count)
            {
                throw new InvalidOperationException(
                    "Embedding provider returned a different number of vectors than inputs.");
            }

            GeneratedEmbedding[] embeddings = data.Select(item =>
            {
                float[] vector = item.Embedding;
                if (vector.Length != model.Dimensions)
                {
                    throw new InvalidOperationException(
                        $"Embedding provider returned {vector.Length} dimensions for '{modelIdentifier}@{modelVersion}', expected {model.Dimensions}.");
                }

                return vector.Any(value => !float.IsFinite(value))
                    ? throw new InvalidOperationException("Embedding provider returned a non-finite vector value.")
                    : new GeneratedEmbedding(vector);
            }).ToArray();
            if (embeddings.Length > 0 && payload.Usage is { PromptTokens: > 0 } usage)
            {
                embeddings[0] = embeddings[0] with { InputUnits = usage.PromptTokens };
            }

            return embeddings;
        }
    }
}