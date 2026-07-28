using Espada.Application.Models;
using Espada.Infrastructure.Services;
using Espada.Tests.Common.Http;
using Espada.Tests.Infrastructure.TestData;
using Microsoft.Extensions.Options;

namespace Espada.Tests.Infrastructure.Embedding
{
    public sealed class OpenAiCompatibleEmbeddingGeneratorServiceTests
    {
        [Fact]
        public async Task GenerateAsync_ShouldCallConfiguredModelAndReturnVector()
        {
            Uri? requestedUri = null;
            string? authorization = null;
            string? requestBody = null;
            using HttpClient client = new(new DelegateHttpMessageHandler(async request =>
            {
                requestedUri = request.RequestUri;
                authorization = request.Headers.Authorization?.ToString();
                requestBody = await request.Content!.ReadAsStringAsync();
                return HttpResponseFactory.Json("""{"data":[{"embedding":[0.1,0.2,0.3]}]}""");
            }));
            OpenAiCompatibleEmbeddingGeneratorService generatorService =
                new(client, Options.Create(EmbeddingTestData.CreateOptions(3)));

            GeneratedEmbedding result = await generatorService.GenerateAsync(
                EmbeddingTestData.ModelIdentifier,
                EmbeddingTestData.ModelVersion,
                EmbeddingTestData.Input,
                TestContext.Current.CancellationToken);

            Assert.Equal(new Uri($"{EmbeddingTestData.BaseUrl}/v1/embeddings"), requestedUri);
            Assert.Equal($"Bearer {EmbeddingTestData.ApiKey}", authorization);
            Assert.Contains($"\"model\":\"{EmbeddingTestData.ProviderModel}\"", requestBody, StringComparison.Ordinal);
            Assert.Equal([0.1f, 0.2f, 0.3f], result.Vector);
        }

        [Fact]
        public async Task GenerateAsync_WhenProviderDimensionsDiffer_ShouldFail()
        {
            using HttpClient client = new(new DelegateHttpMessageHandler(_ => Task.FromResult(HttpResponseFactory.Json(
                """{"data":[{"embedding":[0.1,0.2]}]}"""))));
            OpenAiCompatibleEmbeddingGeneratorService generatorService =
                new(client, Options.Create(EmbeddingTestData.CreateOptions(3)));

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                generatorService.GenerateAsync(
                    EmbeddingTestData.ModelIdentifier,
                    EmbeddingTestData.ModelVersion,
                    EmbeddingTestData.Input,
                    TestContext.Current.CancellationToken));

            Assert.Contains("expected 3", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task GenerateAsync_WhenProviderReturnsNullData_ShouldFailClearly()
        {
            using HttpClient client =
                new(new DelegateHttpMessageHandler(_ =>
                    Task.FromResult(HttpResponseFactory.Json("""{"data":null}"""))));
            OpenAiCompatibleEmbeddingGeneratorService generatorService =
                new(client, Options.Create(EmbeddingTestData.CreateOptions(3)));

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                generatorService.GenerateAsync(
                    EmbeddingTestData.ModelIdentifier,
                    EmbeddingTestData.ModelVersion,
                    EmbeddingTestData.Input,
                    TestContext.Current.CancellationToken));

            Assert.Equal("Embedding provider returned no vector.", exception.Message);
        }
    }
}