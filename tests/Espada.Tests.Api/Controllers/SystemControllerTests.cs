using System.Net;
using System.Text.Json;
using Espada.Tests.Api.Fixtures;
using Espada.Tests.Api.TestData;

namespace Espada.Tests.Api.Controllers;

public sealed class SystemControllerTests(EspadaApiFactory factory) : IClassFixture<EspadaApiFactory>
{
    [Fact]
    public async Task Get_WhenApiIsRunning_ShouldReturnServiceInformation()
    {
        using HttpClient client = factory.CreateHttpsClient();

        HttpResponseMessage response = await client.GetAsync(ApiRoutes.System.Get, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        JsonElement root = document.RootElement;

        Assert.Equal("Espada.Api", root.GetProperty("service").GetString());
        Assert.Equal("running", root.GetProperty("status").GetString());
        Assert.True(root.TryGetProperty("utcNow", out _));
    }

    [Fact]
    public async Task Get_WithoutApiKey_ShouldReturnOk()
    {
        using HttpClient client = factory.CreateHttpsClient(authenticated: false);

        HttpResponseMessage response = await client.GetAsync(ApiRoutes.System.Get, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
