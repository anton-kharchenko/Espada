using System.Net;
using System.Net.Http.Json;
using Espada.Api.Contracts.Requests.ChunkBatches;
using Espada.Api.Contracts.Requests.ChunkEmbeddings;
using Espada.Api.Contracts.Requests.Chunks;
using Espada.Tests.Api.Assertions;
using Espada.Tests.Api.Fixtures;
using Espada.Tests.Api.TestData;

namespace Espada.Tests.Api.Controllers;

public sealed class ChunkApiValidationTests(EspadaApiFactory factory) : IClassFixture<EspadaApiFactory>
{
    [Fact]
    public async Task CreateBatch_WithUnsupportedStrategy_ShouldReturnBadRequest()
    {
        using HttpClient client = factory.CreateHttpsClient();

        CreateChunkBatchRequest request = new()
        {
            StrategyId = int.MaxValue,
            StrategyVersion = TestValues.ChunkingStrategyVersion
        };

        HttpResponseMessage response = await client.PostAsJsonAsync(ApiRoutes.ChunkBatches.Create(TestIds.WorkspaceId, TestIds.ArtifactId, TestIds.ArtifactRevisionId), request, TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
        await response.ShouldContainValidationErrorAsync(nameof(CreateChunkBatchRequest.StrategyId));
    }

    [Fact]
    public async Task CreateChunks_WithEmptyItems_ShouldReturnBadRequest()
    {
        using HttpClient client = factory.CreateHttpsClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(ApiRoutes.Chunks.Create(TestIds.WorkspaceId, TestIds.ChunkBatchId), new CreateChunksRequest(), TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
        await response.ShouldContainValidationErrorAsync(nameof(CreateChunksRequest.Items));
    }

    [Fact]
    public async Task CreateChunks_WithDuplicateNumbers_ShouldReturnBadRequest()
    {
        using HttpClient client = factory.CreateHttpsClient();

        CreateChunksRequest request = new()
        {
            Items =
            [
                new CreateChunkItemRequest { Number = 1, Content = "first" },
                new CreateChunkItemRequest { Number = 1, Content = "second" }
            ]
        };

        HttpResponseMessage response = await client.PostAsJsonAsync(ApiRoutes.Chunks.Create(TestIds.WorkspaceId, TestIds.ChunkBatchId), request, TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
        await response.ShouldContainValidationErrorAsync(nameof(CreateChunksRequest.Items));
    }

    [Fact]
    public async Task CreateEmbedding_WithEmptyVector_ShouldReturnBadRequest()
    {
        using HttpClient client = factory.CreateHttpsClient();

        CreateChunkEmbeddingRequest request = new()
        {
            ModelIdentifier = TestValues.EmbeddingModelIdentifier,
            ModelVersion = TestValues.EmbeddingModelVersion
        };

        HttpResponseMessage response = await client.PostAsJsonAsync(ApiRoutes.ChunkEmbeddings.Create(TestIds.WorkspaceId, TestIds.ChunkId), request, TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
        await response.ShouldContainValidationErrorAsync(nameof(CreateChunkEmbeddingRequest.Vector));
    }
}
