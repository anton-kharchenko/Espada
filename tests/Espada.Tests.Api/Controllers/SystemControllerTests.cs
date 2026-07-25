using System.Net;
using System.Text.Json;
using Espada.Tests.Api.Fixtures;

namespace Espada.Tests.Api.Controllers;

public sealed class SystemControllerTests(EspadaApiFactory factory) : IClassFixture<EspadaApiFactory>
{
    [Fact]
    public async Task Get_WhenApiIsRunning_ShouldReturnServiceInformation()
    {
        using HttpClient client = factory.CreateHttpsClient();

        HttpResponseMessage response = await client.GetAsync("/api/v1/system", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        JsonElement root = document.RootElement;

        Assert.Equal("Espada.Api", root.GetProperty("service").GetString());
        Assert.Equal("running", root.GetProperty("status").GetString());
        Assert.True(root.TryGetProperty("utcNow", out _));
    }
}