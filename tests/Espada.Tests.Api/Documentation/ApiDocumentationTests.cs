using Espada.Tests.Api.Fixtures;
using Espada.Tests.Api.TestData;
using System.Net;
using System.Text.Json;

namespace Espada.Tests.Api.Documentation;

public sealed class ApiDocumentationTests(EspadaApiFactory factory) : IClassFixture<EspadaApiFactory>
{
    [Fact]
    public async Task OpenApi_ShouldDescribeApiKeySecurity()
    {
        using HttpClient client = factory.CreateHttpsClient(authenticated: false);

        HttpResponseMessage response = await client.GetAsync(ApiRouteConstants.OpenApi, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using Stream content = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        using JsonDocument document = await JsonDocument.ParseAsync(content, cancellationToken: TestContext.Current.CancellationToken);
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
        string securityScheme = paths.GetProperty("/api/v1/workspaces/{workspaceId}").GetProperty("get").GetProperty("security")[0].EnumerateObject().Single().Name;

        Assert.Equal("ApiKey", securityScheme);
    }

    [Fact]
    public async Task Scalar_ShouldBePubliclyAvailable()
    {
        using HttpClient client = factory.CreateHttpsClient(authenticated: false);

        HttpResponseMessage response = await client.GetAsync(ApiRouteConstants.Scalar, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
    }
}