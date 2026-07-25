using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Tests.Api.Fixtures;
using Espada.Tests.Api.TestData;

namespace Espada.Tests.Api.Controllers;

public sealed class SourcesControllerValidationTests(EspadaApiFactory factory) : IClassFixture<EspadaApiFactory>
{
    [Fact]
    public async Task Register_WithUnsupportedSourceType_ShouldReturnBadRequest()
    {
        using HttpClient client = factory.CreateHttpsClient();

        Guid workspaceId = Guid.NewGuid();

        object request = new
        {
            name = TestValues.SourceName,
            locator = TestValues.SourceLocator,
            typeId = int.MaxValue
        };

        HttpResponseMessage response = await client.PostAsJsonAsync(ApiRoutes.Sources.Register(workspaceId), request, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        JsonElement errors = document.RootElement.GetProperty("errors");

        Assert.True(errors.TryGetProperty("TypeId", out _));
    }

    [Fact]
    public async Task Register_WithWhiteSpaceName_ShouldReturnBadRequest()
    {
        using HttpClient client = factory.CreateHttpsClient();

        Guid workspaceId = Guid.NewGuid();
        SourceType sourceType = Enumeration.GetAll<SourceType>().First();

        object request = new
        {
            name = " ",
            locator = TestValues.SourceLocator,
            typeId = sourceType.Id
        };

        HttpResponseMessage response = await client.PostAsJsonAsync(ApiRoutes.Sources.Register(workspaceId), request, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        JsonElement errors = document.RootElement.GetProperty("errors");

        Assert.True(errors.TryGetProperty("Name", out _));
    }

    [Fact]
    public async Task Register_WithEmptyLocator_ShouldReturnBadRequest()
    {
        using HttpClient client = factory.CreateHttpsClient();

        Guid workspaceId = Guid.NewGuid();
        SourceType sourceType = Enumeration.GetAll<SourceType>().First();

        object request = new
        {
            name = TestValues.SourceName,
            locator = string.Empty,
            typeId = sourceType.Id
        };

        HttpResponseMessage response = await client.PostAsJsonAsync(ApiRoutes.Sources.Register(workspaceId), request, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        JsonElement errors = document.RootElement.GetProperty("errors");

        Assert.True(errors.TryGetProperty("Locator", out _));
    }
}