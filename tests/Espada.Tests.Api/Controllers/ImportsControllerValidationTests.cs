using Espada.Api.Contracts.Requests.Imports;
using Espada.Tests.Api.Assertions;
using Espada.Tests.Api.Fixtures;
using Espada.Tests.Api.TestData;
using Espada.Tests.Api.TestData.Routes;
using System.Net;
using System.Net.Http.Json;

namespace Espada.Tests.Api.Controllers;

public sealed class ImportsControllerValidationTests(EspadaApiFactory factory)
    : IClassFixture<EspadaApiFactory>
{
    [Fact]
    public async Task Request_WithoutIdempotencyKey_ShouldReturnBadRequest()
    {
        using HttpClient client = factory.CreateHttpsClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            ImportApiRoutes.Request(TestIds.WorkspaceId),
            new RequestImportRequest { SourceId = TestIds.SourceId },
            TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Request_WithEmptySourceId_ShouldReturnBadRequest()
    {
        using HttpClient client = factory.CreateHttpsClient();
        using HttpRequestMessage request = new(
            HttpMethod.Post,
            ImportApiRoutes.Request(TestIds.WorkspaceId));
        request.Headers.Add("Idempotency-Key", "validation-test");
        request.Content = JsonContent.Create(new RequestImportRequest { SourceId = Guid.Empty });

        HttpResponseMessage response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Request_WithInvalidOverlap_ShouldReturnBadRequest()
    {
        using HttpClient client = factory.CreateHttpsClient();
        using HttpRequestMessage request = new(
            HttpMethod.Post,
            ImportApiRoutes.Request(TestIds.WorkspaceId));
        request.Headers.Add("Idempotency-Key", "validation-test");
        request.Content = JsonContent.Create(new RequestImportRequest
        {
            SourceId = TestIds.SourceId,
            Options = new ImportOptionsRequest
            {
                MaxCharacters = 100,
                OverlapCharacters = 100
            }
        });

        HttpResponseMessage response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.BadRequest);
    }
}