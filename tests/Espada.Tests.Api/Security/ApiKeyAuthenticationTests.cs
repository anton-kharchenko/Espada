using Espada.Tests.Api.Fixtures;
using Espada.Tests.Api.TestData;
using System.Net;

namespace Espada.Tests.Api.Security;

public sealed class ApiKeyAuthenticationTests(EspadaApiFactory factory) : IClassFixture<EspadaApiFactory>
{
    [Fact]
    public async Task ProtectedEndpoint_WithoutApiKey_ShouldReturnUnauthorized()
    {
        using HttpClient client = factory.CreateHttpsClient(authenticated: false);

        HttpResponseMessage response = await client.GetAsync(ApiRoutes.Workspaces.GetById(TestIds.WorkspaceId), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}