using System.Net;
using Espada.Tests.E2E.Fixtures;
using Espada.Tests.E2E.TestData;

namespace Espada.Tests.E2E.Api;

[Collection(E2ECollection.Name)]
public sealed class SystemEndpointE2ETests(EspadaE2EFactory factory)
{
    [Fact]
    public async Task GetSystem_WithoutApiKey_ShouldReturnOk()
    {
        using HttpClient client = factory.CreateClient(authenticated: false);

        HttpResponseMessage response = await client.GetAsync(E2ERoutes.System, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}