using Espada.Domain.ValueObjects.SourceDefinitions;
using Espada.Tests.Api.Assertions;
using Espada.Tests.Api.Fixtures;
using Espada.Tests.Api.TestData;
using Espada.Tests.Api.TestData.Routes;
using System.Net;
using System.Net.Http.Json;

namespace Espada.Tests.Api.Controllers;

public sealed class SourcesControllerValidationTests(EspadaApiFactory factory)
    : IClassFixture<EspadaApiFactory>
{
    [Fact]
    public async Task Register_WithoutDefinition_ShouldReturnBadRequest()
    {
        using HttpClient client = factory.CreateHttpsClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            SourceApiRoutes.Register(Guid.NewGuid()),
            new { name = TestValues.SourceName },
            TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithWhiteSpaceName_ShouldReturnBadRequest()
    {
        using HttpClient client = factory.CreateHttpsClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            SourceApiRoutes.Register(Guid.NewGuid()),
            new
            {
                name = " ",
                definition = new PlainTextSourceDefinition("Title", "Content")
            },
            TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithUnknownDefinitionType_ShouldReturnBadRequest()
    {
        using HttpClient client = factory.CreateHttpsClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            SourceApiRoutes.Register(Guid.NewGuid()),
            new
            {
                name = TestValues.SourceName,
                definition = new { type = "executable", path = "tool.exe" }
            },
            TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
    }
}