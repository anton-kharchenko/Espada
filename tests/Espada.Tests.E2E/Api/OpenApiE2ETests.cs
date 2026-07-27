using Espada.Tests.E2E.Fixtures;
using Espada.Tests.E2E.TestData.Constants;
using System.Net;

namespace Espada.Tests.E2E.Api;

[Collection(E2ECollection.Name)]
public sealed class OpenApiE2ETests(EspadaE2EFactory factory) : E2ETest(factory)
{
    [Fact]
    public async Task GetOpenApi_InTestingEnvironment_ShouldReturnDocument()
    {
        using HttpClient client = Factory.CreateClient(authenticated: false);

        HttpResponseMessage response = await client.GetAsync(E2ERouteConstants.OpenApi, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }
}