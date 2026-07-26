using Espada.Tests.E2E.Fixtures;
using Espada.Tests.E2E.TestData;
using System.Net;

namespace Espada.Tests.E2E.Api;

[Collection(E2ECollection.Name)]
public sealed class SystemEndpointE2ETests(EspadaE2EFactory factory) : E2ETest(factory)
{
    [Fact]
    public async Task GetSystem_WithoutApiKey_ShouldReturnOk()
    {
        using HttpClient client = Factory.CreateClient(authenticated: false);

        HttpResponseMessage response = await client.GetAsync(E2ERoutes.System, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}