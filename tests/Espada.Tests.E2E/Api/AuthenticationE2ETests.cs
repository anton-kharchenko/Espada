using Espada.Tests.E2E.Fixtures;
using Espada.Tests.E2E.TestData;
using System.Net;

namespace Espada.Tests.E2E.Api;

[Collection(E2ECollection.Name)]
public sealed class AuthenticationE2ETests(EspadaE2EFactory factory) : E2ETest(factory)
{
    [Fact]
    public async Task ProtectedEndpoint_WithoutApiKey_ShouldReturnUnauthorized()
    {
        using HttpClient client = Factory.CreateClient(authenticated: false);

        HttpResponseMessage response = await client.GetAsync(E2ERoutes.Workspace(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithApiKey_ShouldReachApplication()
    {
        using HttpClient client = Factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(E2ERoutes.Workspace(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithInvalidApiKey_ShouldReturnUnauthorized()
    {
        using HttpClient client = Factory.CreateClient(authenticated: false);
        client.DefaultRequestHeaders.Add(E2ETestValues.ApiKeyHeader, "invalid-api-key");

        HttpResponseMessage response = await client.GetAsync(E2ERoutes.Workspace(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}