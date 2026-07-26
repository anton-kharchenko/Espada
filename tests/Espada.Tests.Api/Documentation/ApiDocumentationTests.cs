using System.Net;
using System.Text.Json;
using Espada.Tests.Api.Fixtures;
using Espada.Tests.Api.TestData;

namespace Espada.Tests.Api.Documentation;

public sealed class ApiDocumentationTests(EspadaApiFactory factory) : IClassFixture<EspadaApiFactory>
{
    [Fact]
    public async Task OpenApi_ShouldDescribeApiKeySecurityAndPublicSystemEndpoint()
    {
        using HttpClient client = factory.CreateHttpsClient(authenticated: false);

        HttpResponseMessage response = await client.GetAsync(ApiRoutes.OpenApi, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using JsonDocument document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken), cancellationToken: TestContext.Current.CancellationToken);
        JsonElement root = document.RootElement;
        JsonElement scheme = root.GetProperty("components").GetProperty("securitySchemes").GetProperty("ApiKey");

        Assert.Equal("apiKey", scheme.GetProperty("type").GetString());
        Assert.Equal("header", scheme.GetProperty("in").GetString());
        Assert.Equal(TestValues.ApiKeyHeader, scheme.GetProperty("name").GetString());

        JsonElement schemas = root.GetProperty("components").GetProperty("schemas");
        Assert.True(schemas.TryGetProperty("CreateChunkEmbeddingRequest", out _));
        Assert.True(schemas.TryGetProperty("CreateChunkEmbeddingResponse", out _));
        Assert.True(schemas.TryGetProperty("ErrorResponse", out _));

        JsonElement paths = root.GetProperty("paths");
        Assert.False(paths.GetProperty("/api/v1/system").GetProperty("get").TryGetProperty("security", out _));
        Assert.Equal("ApiKey", paths.GetProperty("/api/v1/workspaces/{workspaceId}").GetProperty("get").GetProperty("security")[0].EnumerateObject().Single().Name);
    }

    [Fact]
    public async Task Scalar_ShouldBePubliclyAvailable()
    {
        using HttpClient client = factory.CreateHttpsClient(authenticated: false);

        HttpResponseMessage response = await client.GetAsync(ApiRoutes.Scalar, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
    }
}
