using System.Net;
using Espada.Tests.E2E.Fixtures;
using Espada.Tests.E2E.TestData;

namespace Espada.Tests.E2E.Api;

[Collection(E2ECollection.Name)]
public sealed class OpenApiE2ETests(EspadaE2EFactory factory)
{
    [Fact]
    public async Task GetOpenApi_InTestingEnvironment_ShouldReturnDocument()
    {
        using HttpClient client = factory.CreateClient(authenticated: false);

        HttpResponseMessage response = await client.GetAsync(E2ERoutes.OpenApi, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }
}