using System.Net;
using Espada.Tests.E2E.Fixtures;
using Espada.Tests.E2E.TestData;

namespace Espada.Tests.E2E.Api;

[Collection(E2ECollection.Name)]
public sealed class AuthenticationE2ETests(EspadaE2EFactory factory)
{
    [Fact]
    public async Task ProtectedEndpoint_WithoutApiKey_ShouldReturnUnauthorized()
    {
        using HttpClient client = factory.CreateClient(authenticated: false);

        HttpResponseMessage response = await client.GetAsync(E2ERoutes.Workspace(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithApiKey_ShouldReachApplication()
    {
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(E2ERoutes.Workspace(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}